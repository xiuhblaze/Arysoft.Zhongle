using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Arysoft.ProyectoN.Models;
using Microsoft.AspNet.Identity;
using System.Dynamic;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class SectorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Sector
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string status)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenResponsable = orden == "responsable" ? "responsable_desc" : "responsable";
                        
            if (buscar != null)
            {
                //pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var sectores = db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Secciones.Select(se => se.Personas))
                .Include(s => s.Secciones.Select(se => se.Casillas))
                .Include(s => s.Responsable)
                .Where(s => s.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar) || !string.IsNullOrEmpty(status))
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);
                status = string.IsNullOrEmpty(status) ? StatusTipo.Ninguno.ToString() : status;

                sectores = sectores.Where(s =>
                    (
                        !hayPalabras
                        || s.Nombre.Contains(buscar)
                        || s.Responsable.Nombres.Contains(buscar)
                        || s.Responsable.ApellidoPaterno.Contains(buscar)
                        || s.Responsable.ApellidoMaterno.Contains(buscar)
                        || s.Descripcion.Contains(buscar)
                    )
                    && (status == StatusTipo.Ninguno.ToString() ? true : s.Status.ToString() == status)
                );
            }

            // Filtra por el rol asignado al usuario, para solo mostrar registros de acuerdo a su nivel
            if (ControllerContext.HttpContext.User.IsInRole("Consultant"))
            {
                sectores = sectores.Where(s => s.Status == StatusTipo.Activo);
            }
            else if (ControllerContext.HttpContext.User.IsInRole("Editor"))
            {
                sectores = sectores.Where(s => s.Status == StatusTipo.Activo || s.Status == StatusTipo.Baja);
            }

            switch (orden)
            {
                case "nombre_desc":
                    sectores = sectores.OrderByDescending(s => s.Nombre);
                    break;
                case "responsable":
                    sectores = sectores.OrderBy(s => s.Responsable.Nombres);
                    break;
                case "responsable_desc":
                    sectores = sectores.OrderByDescending(s => s.Responsable.Nombres);
                    break;
                default:
                    sectores = sectores.OrderBy(s => s.Nombre);
                    break;
            }

            return View(await sectores.ToListAsync());
        } // GET: Index

        // GET: Sector/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest())
                {
                    return Content("noid");
                }
                return RedirectToAction("Index");
            }
            Sector sector = await db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Secciones.Select(se => se.Personas))
                .Include(s => s.Secciones.Select(se => se.Casillas))
                .Include(s => s.Secciones.Select(se => se.Casillas.Select(cas => cas.Votantes)))                
                .Include(s => s.Personas)
                //.Include(s => s.Personas.Select(p => p.UbicacionVive))                
                //.Include(s => s.Personas.Select(p => p.UbicacionVota))                
                .Include(s => s.Personas.Select(p => p.PersonasAfines))
                .Include(s => s.Personas.Select(p => p.Notas))
                .FirstOrDefaultAsync(s => s.SectorID == id);
            if (sector == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest())
                {
                    return Content("nofound");
                }
                return RedirectToAction("Index");
            }

            if (sector.Secciones != null) sector.Secciones = sector.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero).ToList();
            if (sector.Personas != null) sector.Personas = sector.Personas.Where(p => p.Status == StatusTipo.Activo).OrderBy(p => p.Nombres).ToList();

            sector.SoloLectura = true;

            if (Request.IsAjaxRequest())
            {
                sector.NmOrigen = "details";
                return PartialView("_details", sector);
            }

            return View(sector);
        }

        // GET: Sector/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(User.Identity.Name);
            
            Sector sector = new Sector();

            sector.SectorID = Guid.NewGuid();
            sector.ResponsableID = Guid.Empty;
            sector.Nombre = string.Empty;
            sector.Status = StatusTipo.Ninguno;
            sector.UserNameActualizacion = User.Identity.Name; // string.Empty;
            sector.FechaActualizacion = DateTime.Now;

            db.Sectores.Add(sector);
            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = sector.SectorID });
            }
            // Código tomado de http://www.c-sharpcorner.com/UploadFile/97fc7a/validation-failed-for-one-or-more-entities-mvcentity-frame/
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                Exception raise = dbEx;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}: {1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        // raise a new exception nesting  
                        // the current instance as InnerException  
                        raise = new InvalidOperationException(message, raise);
                    }
                }
                throw raise;
            }
        } // Create

        // GET: Sector/Edit/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");                
            }
            
            db.Database.CommandTimeout = 180;

            Sector sector = await db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Secciones.Select(se => se.Personas))
                .Include(s => s.Secciones.Select(se => se.Casillas))
                .Include(s => s.Secciones.Select(se => se.Casillas.Select(cas => cas.Votantes)))                
                .Include(s => s.Personas)
                .Include(s => s.Personas.Select(p => p.UbicacionVive))
                .Include(s => s.Personas.Select(p => p.UbicacionVive.Calle))
                .Include(s => s.Personas.Select(p => p.UbicacionVive.Colonia))
                .Include(s => s.Personas.Select(p => p.UbicacionVota))
                .Include(s => s.Personas.Select(p => p.UbicacionVota.Calle))
                .Include(s => s.Personas.Select(p => p.UbicacionVota.Colonia))
                .Include(s => s.Personas.Select(p => p.PersonasAfines))
                .Include(s => s.Personas.Select(p => p.Notas))
                .FirstOrDefaultAsync(s => s.SectorID == id);

            if (sector == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");                
            }

            if (sector.Secciones != null) sector.Secciones = sector.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero).ToList();
            if (sector.Personas != null) sector.Personas = sector.Personas.Where(p => p.Status == StatusTipo.Activo).OrderBy(p => p.Nombres).ToList();

            ViewBag.ResponsableID = await ObtenerListaPersonasAfiliadasAsync(sector.ResponsableID ?? Guid.Empty);
            
            return View(sector);
        } // GET: Edit

        // POST: Sector/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit([Bind(Include = "SectorID,ResponsableID,Nombre,Descripcion,VotosMeta,Status,UserNameActualizacion,FechaActualizacion")] Sector sector)
        {
            bool esNuevo = sector.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {                
                sector.Status = StatusTipo.Activo;
            }

            if (sector.ResponsableID == null) sector.ResponsableID = Guid.Empty;

            if (ModelState.IsValid)
            {
                sector.UserNameActualizacion = User.Identity.Name;
                sector.FechaActualizacion = DateTime.Now;
                db.Entry(sector).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }

            if (sector.Secciones != null) sector.Secciones = sector.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero).ToList();
            if (sector.Personas != null) sector.Personas = sector.Personas.Where(p => p.Status == StatusTipo.Activo).OrderBy(p => p.Nombres).ToList();
            ViewBag.ResponsableID = await ObtenerListaPersonasAfiliadasAsync(sector.ResponsableID ?? Guid.Empty);

            return View(sector);
        } // POST: Edit

        // GET: Sector/Delete/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
            }
            Sector sector = await db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Secciones.Select(se => se.Personas))
                .Include(s => s.Secciones.Select(se => se.Casillas))
                .Include(s => s.Secciones.Select(se => se.Casillas.Select(cas => cas.Votantes)))
                .Include(s => s.Personas)
                //.Include(s => s.Personas.Select(p => p.UbicacionVive))                
                //.Include(s => s.Personas.Select(p => p.UbicacionVota))                
                .Include(s => s.Personas.Select(p => p.PersonasAfines))
                .Include(s => s.Personas.Select(p => p.Notas))
                .FirstOrDefaultAsync(s => s.SectorID == id);
            if (sector == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }
            sector.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                sector.NmOrigen = "delete";
                return PartialView("_details", sector);
            }
            return View(sector);
        }

        // POST: Sector/Delete/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            bool sePuedeEliminar = true;

            Sector sector = await db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Personas)
                .FirstOrDefaultAsync(s => s.SectorID == id);

            if (sector.Secciones != null && sector.Secciones.Count > 0) { sePuedeEliminar = false; }
            if (sector.Personas != null && sector.Personas.Count > 0) { sePuedeEliminar = false; }

            switch (sector.Status)
            {
                case StatusTipo.Activo:
                    sector.Status = StatusTipo.Baja;
                    sector.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                    sector.FechaActualizacion = DateTime.Now;
                    db.Entry(sector).State = EntityState.Modified;
                    break;
                case StatusTipo.Baja:
                    sector.Status = StatusTipo.Eliminado;
                    sector.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                    sector.FechaActualizacion = DateTime.Now;
                    db.Entry(sector).State = EntityState.Modified;
                    break;
                case StatusTipo.Eliminado:
                    if (sePuedeEliminar)
                    {
                        db.Sectores.Remove(sector);
                    }
                    break;
            }

            sector.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            sector.FechaActualizacion = DateTime.Now;

            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        } // DeleteConfirmed

        // POST: Sector/Activar/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activar(Guid? id)
        {
            Sector sector = await db.Sectores.FindAsync(id);
            if (sector == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                return RedirectToAction("Index");
            }
            sector.Status = StatusTipo.Activo;
            sector.UserNameActualizacion = User.Identity.Name;
            sector.FechaActualizacion = DateTime.Now;
            db.Entry(sector).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "El sector ha sido activado.";

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
        ////////////////////////////////////////////////////////////////////////////////

        public async Task<ActionResult> VotosMetaVSAlcanzados() {
            var sectores = await db.Sectores
                .Include(s => s.Secciones.Select(sc => sc.Personas))
                .Include(s => s.Personas.Select(pe => pe.PersonasAfines))
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Nombre).ToListAsync();
            //var sectores = from s in db.Sectores
            //               where s.Status == StatusTipo.Activo
            //               orderby s.Nombre
            //               select s;

            var tabla = new List<object>();

            foreach (var sector in sectores)
            {
                var votosAlcanzados = 0;
                var votosTrabajados = 0;

                foreach (var seccion in sector.Secciones)
                {
                    votosAlcanzados += seccion.Personas
                        .Where(p => p.VotanteSeguro == BoolTipo.Si && p.Status == StatusTipo.Activo)
                        .Count();
                }

                foreach (var brigadista in sector.Personas)
                {
                    votosTrabajados += brigadista.PersonasAfines
                        .Where(p => p.VotanteSeguro == BoolTipo.Si && p.Status == StatusTipo.Activo)
                        .Count();
                }

                tabla.Add(new
                {
                    Sector = sector.Nombre,
                    Trabajados = votosTrabajados,
                    Alcanzados = votosAlcanzados,
                    Meta = sector.VotosMeta
                });
            }

            return Json(tabla, JsonRequestBehavior.AllowGet);
        } // VotosMetaVSAlcanzados

        public async Task<ActionResult> VotosPorSectorTiempo() {
            //var sectores = await (from s in db.Sectores
            //               where s.Status == StatusTipo.Activo
            //               orderby s.Nombre
            //               select s).ToListAsync();

            var sectores = await db.Sectores
                .Include(s => s.Secciones)
                .Include(s => s.Secciones.Select(se => se.Personas))
                .Where(s => s.Status == StatusTipo.Activo)
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            var tabla = new List<object>();

            int semanaNo = 18;
            DateTime fechaInicio = new DateTime(2021, 4, 4);
            DateTime fechaTermino = new DateTime(2021, 6, 6);
            DateTime semanaActual = fechaInicio;

            // https://stackoverflow.com/questions/12427280/creating-dynamic-objects
            var testData = new List<ExpandoObject>();

            while (semanaActual < DateTime.Today.AddDays(7) && semanaActual < fechaTermino)
            {
                DateTime semanaFechaInicio = semanaActual.AddDays(-((semanaActual.DayOfWeek - System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7)).Date;
                DateTime semanaFechaTermino = semanaFechaInicio.AddDays(6);

                dynamic data = new ExpandoObject();

                string semana = semanaFechaInicio.ToString("dd/MMM") + "-" + semanaFechaTermino.ToString("dd/MMM");
                semana = semana.Replace(".", "");

                ((IDictionary<string, Object>)data).Add("Semanas", semana);
                foreach (var sector in sectores) {
                    int votosSector = 0;

                    if (sector.Secciones != null)
                    {
                        foreach (var seccion in sector.Secciones)
                        {
                            if (seccion.Status == StatusTipo.Activo)
                            {
                                votosSector += seccion.Personas
                                    .Where(p => p.Status == StatusTipo.Activo
                                        && p.VotanteSeguro == BoolTipo.Si
                                        && p.FechaAlta <= semanaActual)
                                    .Count();
                            }
                        }
                    }

                    ((IDictionary<string, Object>)data).Add(sector.Nombre, votosSector);

                    //i++;
                }
                semanaNo++;
                testData.Add(data);
                semanaActual = semanaActual.AddDays(7);
            }

            return Json(testData, JsonRequestBehavior.AllowGet);
        } // VotosPorSectorTiempo

        // METODOS PRIVADOS
        ////////////////////////////////////////////////////////////////////////////////

        private async Task<List<SelectListItem>> ObtenerListaPersonasAfiliadasAsync(Guid PersonaSeleccionadaID)
        {            
            List<SelectListItem> listado = new List<SelectListItem>();

            listado.Add(new SelectListItem { Text = "(seleccionar responsable)", Value = Guid.Empty.ToString() });

            var personas = await db.Personas
                .Where(p => p.Status == StatusTipo.Activo 
                    && (p.Afinidad == AfinidadTipo.Movilizador || p.Afinidad == AfinidadTipo.Simpatizante))
                .OrderBy(p => p.ApellidoPaterno)
                .ThenBy(p => p.ApellidoMaterno)
                .ToListAsync();

            foreach (var item in personas)
            {
                listado.Add(new SelectListItem
                {
                    Text = item.NombreCompleto,
                    Value = item.PersonaID.ToString(),
                    Selected = (item.PersonaID == PersonaSeleccionadaID)
                });
            }

            return listado;
        } // ObtenerListaPersonasAfiliadas

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var sectores = await (from s in db.Sectores
                            where s.Status == StatusTipo.Ninguno
                                && string.Compare(s.UserNameActualizacion, userName, true) == 0
                            select s).ToListAsync();

            foreach (Sector s in sectores)
            {
                db.Sectores.Remove(s);
            }

            await db.SaveChangesAsync();
        }
    }
}
