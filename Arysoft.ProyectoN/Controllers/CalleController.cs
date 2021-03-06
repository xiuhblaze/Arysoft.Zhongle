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
    public class CalleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Calle
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string status, int? pagina)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var calles = db.Calles
                .Include(c => c.Colonias)
                .Where(c => c.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar) || !string.IsNullOrEmpty(status))
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);

                status = string.IsNullOrEmpty(status) ? StatusTipo.Ninguno.ToString() : status;
                calles = calles.Where(c =>
                    c.Nombre.Contains(buscar)
                //|| (from col in c.Colonias where col.Nombre.Contains(buscar) select c).ToList().Count > 0
                );
            }
            else 
            {
                if (ControllerContext.HttpContext.User.IsInRole("Admin"))
                {
                    calles = calles.Where(m => m.Status != StatusTipo.Ninguno);
                }
                else if (ControllerContext.HttpContext.User.IsInRole("Editor"))
                {
                    calles = calles.Where(m => m.Status == StatusTipo.Activo || m.Status == StatusTipo.Baja);
                }
                else if (ControllerContext.HttpContext.User.IsInRole("Consultant"))
                {
                    calles = calles.Where(m => m.Status == StatusTipo.Activo);
                }
            }

            switch (orden)
            {
                case "nombre_desc":
                    calles = calles.OrderByDescending(c => c.Nombre);
                    break;                
                default:
                    calles = calles.OrderBy(c => c.Nombre);
                    break;
            }

            List<CalleListViewModel> callesList = new List<CalleListViewModel>();
            foreach (var calle in await calles.ToListAsync())
            {
                callesList.Add(new CalleListViewModel(calle));
            }

            ViewBag.Count = callesList.Count();
            int numeroPagina = pagina ?? 1;
            int elementosPagina = 50;

            //return View(callesList);
            return View(callesList.ToPagedList(numeroPagina, elementosPagina));
        } // Index

        // GET: Calle/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
            }
            Calle calle = await db.Calles
                .Include(c => c.Colonias)
                .Include(c => c.Colonias.Select(col => col.Seccion))
                .Include(c => c.Colonias.Select(col => col.Poblacion))
                .Where(c => c.CalleID == id)
                .FirstOrDefaultAsync();
            if (calle == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }

            calle.NpSoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                calle.NpOrigen = "details";
                return PartialView("_details", calle);
            }

            return View(calle);
        } // Details

        // GET: Calle/Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Calle calle = new Calle();

            calle.CalleID = Guid.NewGuid();
            calle.Status = StatusTipo.Ninguno;
            calle.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            calle.FechaActualizacion = DateTime.Now;

            db.Calles.Add(calle);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = calle.CalleID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
        } // Create

        // GET: Calle/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Calle calle = await db.Calles
                .Include(c => c.Colonias)
                .Include(c => c.Colonias.Select(col => col.Seccion))
                .Include(c => c.Colonias.Select(col => col.Poblacion))
                .FirstOrDefaultAsync(c => c.CalleID == id);
            if (calle == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            calle.NpSoloLectura = false;

            return View(calle);
        } // Edit

        // POST: Calle/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "CalleID,Nombre,Status,UserNameActualizacion,FechaActualizacion")] Calle calle)
        {
            bool esNuevo = calle.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                calle.Status = StatusTipo.Activo;
            }

            if (ModelState.IsValid)
            {
                calle.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                calle.FechaActualizacion = DateTime.Now;
                db.Entry(calle).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }
            calle.NpSoloLectura = false;

            return View(calle);
        } // Edit

        // GET: Calle/Delete/5
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
            Calle calle = await db.Calles
                .Include(c => c.Colonias)
                .Include(c => c.Colonias.Select(col => col.Seccion))
                .Include(c => c.Colonias.Select(col => col.Poblacion))
                .Where(c => c.CalleID == id)
                .FirstOrDefaultAsync();
            if (calle == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }
            calle.NpSoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                calle.NpOrigen = "delete";
                return PartialView("_details", calle);
            }
            return View(calle);
        } // Delete

        // POST: Calle/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Calle calle = await db.Calles.FindAsync(id);

            if (calle.Status == StatusTipo.Activo)
            {
                calle.Status = StatusTipo.Baja;
                calle.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                calle.FechaActualizacion = DateTime.Now;
                db.Entry(calle).State = EntityState.Modified;                
            }
            else if (calle.Status == StatusTipo.Baja)
            {
                calle.Status = StatusTipo.Eliminado;
                calle.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                calle.FechaActualizacion = DateTime.Now;
                db.Entry(calle).State = EntityState.Modified;                
            }
            else
            {
                db.Calles.Remove(calle);            
            }
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        } // DeleteConfirmed

        // POST: Calle/Activar/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activar(Guid id)
        {
            Calle calle = await db.Calles.FindAsync(id);
            if (calle == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                return RedirectToAction("Index");
            }
            calle.Status = StatusTipo.Activo;
            calle.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            calle.FechaActualizacion = DateTime.Now;
            db.Entry(calle).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "La calle ha sido reactivada.";

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
        public async Task<ActionResult> EliminarColonia(Guid id, Guid calleID)
        {
            Calle calle = await db.Calles.Include(c => c.Colonias).FirstOrDefaultAsync(c => c.CalleID == calleID);
            Colonia colonia = await db.Colonias.FindAsync(id);

            calle.Colonias.Remove(colonia);            
            db.Entry(calle).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return PartialView("_listaColonias", calle);
        } // EliminarColonia

        // METODOS PRIVADOS

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            //using (var db2 = new ApplicationDbContext()) 
            //{
                var calles = await (from c in db.Calles
                                    where c.Status == StatusTipo.Ninguno
                                     && string.Compare(c.UserNameActualizacion, userName, true) == 0
                                    select c).ToListAsync();

                foreach (Calle calle in calles)
                {
                    db.Calles.Remove(calle);
                }
                await db.SaveChangesAsync();
            //}
            
        } // EliminarRegistrosTemporales
    }
}
