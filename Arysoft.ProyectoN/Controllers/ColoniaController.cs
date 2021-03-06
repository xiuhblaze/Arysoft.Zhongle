using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Arysoft.ProyectoN.Models;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class ColoniaController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Colonia
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string status, int? pagina)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenSeccion = orden == "seccion" ? "seccion_desc" : "seccion";
            ViewBag.OrdenCP = orden == "cp" ? "cp_desc" : "cp";

            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var colonias = db.Colonias
                .Include(c => c.Poblacion)
                .Include(c => c.Seccion)
                .Include(c => c.Calles)
                .Where(c => c.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar))
            {
                int.TryParse(buscar, out int seccion);

                colonias = colonias.Where(c =>
                    c.Nombre.Contains(buscar)
                    || c.CodigoPostal.Contains(buscar)
                    || c.Poblacion.Nombre.Contains(buscar)
                    && 
                    seccion == 0 ? true : c.Seccion.Numero == seccion
                );
            }

            switch (orden) {
                case "nombre_desc":
                    colonias = colonias.OrderByDescending(c => c.Nombre);
                    break;
                case "seccion":
                    colonias = colonias.OrderBy(c => c.Seccion.Numero).ThenBy(c => c.Nombre);
                    break;
                case "seccion_desc":
                    colonias = colonias.OrderByDescending(c => c.Seccion.Numero).ThenBy(c => c.Nombre);
                    break;
                case "cp":
                    colonias = colonias.OrderBy(c => c.CodigoPostal).ThenBy(c => c.Nombre);
                    break;
                case "cp_desc":
                    colonias = colonias.OrderByDescending(c => c.CodigoPostal).ThenBy(c => c.Nombre);
                    break;
                default:
                    colonias = colonias.OrderBy(c => c.Nombre);
                    break;
            }

            List<Colonia> coloniasList = await colonias.ToListAsync();

            ViewBag.Count = coloniasList.Count();

            int numeroPagina = pagina ?? 1;
            int elementosPagina = 50;

            return View(coloniasList.ToPagedList(numeroPagina, elementosPagina)); //await colonias.ToListAsync());
        } // GET: Index

        // GET: Colonia/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
            }
            Colonia colonia = await db.Colonias
                .Include(c => c.Poblacion)
                .Include(c => c.Seccion)
                .Include(c => c.Calles)
                .Where(c => c.ColoniaID == id)
                .FirstOrDefaultAsync();
            if (colonia == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }

            colonia.NpSoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                colonia.NpOrigen = "details";
                return PartialView("_details", colonia);
            }

            return View(colonia);
        } // Details

        // GET: Colonia/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Colonia colonia = new Colonia();

            colonia.ColoniaID = Guid.NewGuid();
            colonia.PoblacionID = new Guid("a522424c-14ab-4f8d-bc52-66b1bae9c42c"); // por default Ciudad Guzmán
            colonia.Status = StatusTipo.Ninguno;
            colonia.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            colonia.FechaActualizacion = DateTime.Now;

            db.Colonias.Add(colonia);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = colonia.ColoniaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

            //ViewBag.PoblacionID = new SelectList(db.Poblaciones, "PoblacionID", "Nombre");
            //ViewBag.SeccionID = new SelectList(db.Secciones, "SeccionID", "SeccionID");
            //return View();
        }

        // GET: Colonia/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Colonia colonia = await db.Colonias
                .Include(c => c.Calles)
                .Include(c => c.Poblacion)
                .FirstOrDefaultAsync(c => c.ColoniaID == id);
            if (colonia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }
            //if (colonia.Poblacion.MunicipioID != Guid.Empty)
            //{
            //    colonia.Poblacion.Municipio = await db.Municipios.FindAsync(colonia.Poblacion.MunicipioID);
            //}
            colonia.NpSoloLectura = false;
            ViewBag.PoblacionID = await ObtenerListaPoblacionesAsync(colonia.PoblacionID);
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(colonia.SeccionID ?? Guid.Empty);
            ViewBag.Calles = await ObtenerListaCallesAsync();
            // ViewBag.Municipios = await ObtenerListaMunicipiosAsync(colonia.Poblacion.MunicipioID);

            return View(colonia);
        }

        // POST: Colonia/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "ColoniaID,PoblacionID,SeccionID,Nombre,CodigoPostal,Status,UserNameActualizacion,FechaActualizacion")] Colonia colonia)
        {
            bool esNuevo = colonia.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                colonia.Status = StatusTipo.Activo;
            }

            if (ModelState.IsValid)
            {
                colonia.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                colonia.FechaActualizacion = DateTime.Now;
                db.Entry(colonia).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }
            colonia.NpSoloLectura = false;
            ViewBag.PoblacionID = await ObtenerListaPoblacionesAsync(colonia.PoblacionID);
            ViewBag.SeccionID = await ObtenerListaSeccionesAsync(colonia.SeccionID ?? Guid.Empty);
            ViewBag.Calles = await ObtenerListaCallesAsync();
            // ViewBag.Municipios = await ObtenerListaMunicipiosAsync(colonia.Poblacion.MunicipioID);

            return View(colonia);
        }

        // GET: Colonia/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
            }
            Colonia colonia = await db.Colonias
                .Include(c => c.Poblacion)
                .Include(c => c.Seccion)
                .Include(c => c.Calles)
                .Where(c => c.ColoniaID == id)
                .FirstOrDefaultAsync();
            if (colonia == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }
            colonia.NpSoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                colonia.NpOrigen = "delete";
                return PartialView("_details", colonia);
            }
            return View(colonia);
        }

        // POST: Colonia/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Colonia colonia = await db.Colonias.FindAsync(id);

            if (colonia.Status == StatusTipo.Activo)
            {
                colonia.Status = StatusTipo.Baja;
                colonia.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                colonia.FechaActualizacion = DateTime.Now;
                db.Entry(colonia).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else if (colonia.Status == StatusTipo.Baja)
            {
                colonia.Status = StatusTipo.Eliminado;
                colonia.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                colonia.FechaActualizacion = DateTime.Now;
                db.Entry(colonia).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            else
            {
                db.Colonias.Remove(colonia);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: Colonia/Activar/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activar(Guid id)
        {
            Colonia colonia = await db.Colonias.FindAsync(id);
            if (colonia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                return RedirectToAction("Index");
            }
            colonia.Status = StatusTipo.Activo;
            colonia.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            colonia.FechaActualizacion = DateTime.Now;
            db.Entry(colonia).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "La colonia ha sido reactivada.";
            return RedirectToAction("Index");
        } // Activar

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // METODOS AJAX

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AgregarCalle(Guid id, Guid coloniaID)
        {
            Colonia colonia = await db.Colonias.Include(c => c.Calles).FirstOrDefaultAsync(c => c.ColoniaID == coloniaID);
            Calle calle = await db.Calles.FindAsync(id);

            colonia.Calles.Add(calle);
            db.Entry(colonia).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return PartialView("_listaCalles", colonia);
        } // AgregarCalle

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EliminarCalle(Guid id, Guid coloniaID)
        {
            Colonia colonia = await db.Colonias.Include(c => c.Calles).FirstOrDefaultAsync(c => c.ColoniaID == coloniaID);
            Calle calle = await db.Calles.FindAsync(id);

            colonia.Calles.Remove(calle);
            db.Entry(colonia).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return PartialView("_listaCalles", colonia);
        } // EliminarCalle

        // METODOS PRIVADOS

        //private List<SelectListItem> ObtenerListaPoblaciones(Guid selectedId)
        //{
        //    var listado = new SelectList(db.Poblaciones
        //        .Where(p => p.MunicipioID == new Guid("fd1c7699-4dd5-479b-a840-28fbf3f81fdf")), 
        //        "PoblacionID", "Nombre", selectedId).ToList(); // Solo poblaciones de Zapotlán El Grande
        //    listado.Insert(0, (new SelectListItem { Text = "(seleccionar población)", Value = Guid.Empty.ToString() }));

        //    return listado;
        //} // ObtenerListaPoblaciones

        //private List<SelectListItem> ObtenerListaSecciones(Guid selectedId)
        //{
        //    var listado = new SelectList(db.Secciones
        //        .Where(s => s.Status == StatusTipo.Activo), "SeccionID", "Numero", selectedId).ToList();
        //    listado.Insert(0, (new SelectListItem { Text = "(seleccionar sección)", Value = Guid.Empty.ToString() }));

        //    return listado;
        //}

        //private List<SelectListItem> ObtenerListaCalles()
        //{
        //    var listado = new SelectList(db.Calles
        //        .Where(c => c.Status == StatusTipo.Activo)
        //        .OrderBy(c => c.Nombre), "CalleID", "Nombre").ToList();
        //    listado.Insert(0, (new SelectListItem { Text = "(seleccionar calle)", Value = Guid.Empty.ToString() }));

        //    return listado;
        //} // ObtenerListaPersonasAfiliadas

        // METODOS PRIVADOS ASYNC

        //private async Task<List<SelectListItem>> ObtenerListaMunicipiosAsync(Guid selectedID)
        //{
        //    var municipios = await (db.Municipios
        //        .Where(m => m.Status == StatusTipo.Activo)
        //        .OrderBy(m => m.Nombre)).ToListAsync();
        //    var listado = new SelectList(municipios, "MunicipioID", "Nombre", selectedID).ToList();
        //    listado.Insert(0, new SelectListItem { Text = "(municipio)", Value = Guid.Empty.ToString() });

        //    return listado;
        //} // ObtenerListaMunicipiosAsync

        private async Task<List<SelectListItem>> ObtenerListaPoblacionesAsync(Guid selectedId)
        {
            var poblaciones = await (db.Poblaciones
                .Where(p => p.Status == StatusTipo.Activo
                    && p.MunicipioID == new Guid("fd1c7699-4dd5-479b-a840-28fbf3f81fdf"))).ToListAsync(); // Solo poblaciones de Zapotlán El Grande
            var listado = new SelectList(poblaciones, "PoblacionID", "Nombre", selectedId).ToList(); 
            listado.Insert(0, (new SelectListItem { Text = "(seleccionar población)", Value = Guid.Empty.ToString() }));

            return listado;
        } // ObtenerListaPoblacionesAsync

        private async Task<List<SelectListItem>> ObtenerListaSeccionesAsync(Guid selectedId)
        {
            var listado = new SelectList(await (db.Secciones
                .Where(s => s.Status == StatusTipo.Activo)
                .ToListAsync()), "SeccionID", "Numero", selectedId).ToList();
            listado.Insert(0, (new SelectListItem { Text = "(seleccionar sección)", Value = Guid.Empty.ToString() }));

            return listado;
        }

        private async Task<List<SelectListItem>> ObtenerListaCallesAsync()
        {
            var listado = new SelectList(await (db.Calles
                .Where(c => c.Status == StatusTipo.Activo)
                .OrderBy(c => c.Nombre)).ToListAsync(), "CalleID", "Nombre").ToList();
            listado.Insert(0, (new SelectListItem { Text = "(seleccionar calle)", Value = Guid.Empty.ToString() }));

            return listado;
        } // ObtenerListaPersonasAfiliadas

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var colonias = await (from c in db.Colonias
                            where c.Status == StatusTipo.Ninguno
                             && string.Compare(c.UserNameActualizacion, userName, true) == 0
                            select c).ToListAsync();

            foreach (Colonia col in colonias)
            {
                db.Colonias.Remove(col);
            }
            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
    }
}
