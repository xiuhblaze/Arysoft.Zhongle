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

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class SeccionController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Seccion
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNumero = string.IsNullOrEmpty(orden) ? "numero_desc" : "";
            ViewBag.OrdenSector = orden == "sector" ? "sector_desc" : "sector";
            ViewBag.OrdenCasillas = orden == "casillas" ? "casillas_desc" : "casillas";
            ViewBag.OrdenColonias = orden == "colonias" ? "colonias_desc" : "colonias";
            ViewBag.OrdenVotantes = orden == "votantes" ? "votantes_desc" : "votantes";

            if (buscar != null)
            {
                //pagina = 1
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var secciones = db.Secciones
                .Include(s => s.Sector)
                .Include(s => s.Personas)
                .Include(s => s.Casillas)
                .Where(s => s.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar))
            {
                int.TryParse(buscar, out int numero);

                secciones = secciones.Where(c =>
                    c.Descripcion.Contains(buscar)
                    &&
                    numero == 0 ? true : c.Numero == numero
                );
            }

            var seccionesList = await secciones.ToListAsync();

            switch (orden)
            {
                case "numero_desc":
                    seccionesList = seccionesList.OrderByDescending(s => s.Numero).ToList();
                    break;
                case "sector":
                    seccionesList = seccionesList.OrderBy(s => s.Sector.Nombre).ThenBy(s => s.Numero).ToList();
                    break;
                case "sector_desc":
                    seccionesList = seccionesList.OrderByDescending(s => s.Sector.Nombre).ThenBy(s => s.Numero).ToList();
                    break;
                case "casillas":
                    seccionesList = seccionesList.OrderBy(s => s.Casillas == null).ThenBy(s => s.Casillas.Count()).ToList();
                    break;
                case "casillas_desc":
                    seccionesList = seccionesList.OrderByDescending(s => s.Casillas == null).ThenByDescending(s => s.Casillas.Count()).ToList();
                    break;
                case "colonias":
                    seccionesList = seccionesList.OrderBy(s => s.Colonias == null).ThenBy(s => s.Colonias.Count()).ToList();
                    break;
                case "colonias_desc":
                    seccionesList = seccionesList.OrderByDescending(s => s.Colonias == null).ThenByDescending(s => s.Colonias.Count()).ToList();
                    break;
                case "votantes":
                    seccionesList = seccionesList.OrderBy(s => s.Personas == null).ThenBy(s => s.Personas.Count()).ToList();
                    break;
                case "votantes_desc":
                    seccionesList = seccionesList.OrderByDescending(s => s.Personas == null).ThenByDescending(s => s.Personas.Count()).ToList();
                    break;
                default:
                    seccionesList = seccionesList.OrderBy(s => s.Numero).ToList();
                    break;
            }

            return View(seccionesList); //secciones.ToList());
        } // GET: Index

        // GET: Seccion/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("noid"); }
                return RedirectToAction("Index");
            }
            Seccion seccion = await db.Secciones
                .Include(s => s.Casillas)
                .Include(s => s.Colonias)
                .Include(s => s.Personas)
                .Include(s => s.Personas.Select(p => p.Notas))
                .Include(s => s.Sector)
                .Where(s => s.SeccionID == id)
                .FirstOrDefaultAsync();
            if (seccion == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("nofound"); }
                return RedirectToAction("Index");
            }

            if (seccion.Casillas != null) seccion.Casillas = seccion.Casillas.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Tipo).ToList();
            if (seccion.Colonias != null) seccion.Colonias = seccion.Colonias.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Nombre).ToList();

            seccion.SoloLectura = true;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_details", seccion);
            }

            return View(seccion);
        } // Details

        // GET: Seccion/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Seccion seccion = new Seccion();

            seccion.SeccionID = Guid.NewGuid();
            seccion.SectorID = Guid.Empty;
            seccion.Status = StatusTipo.Ninguno;
            seccion.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
            seccion.FechaActualizacion = DateTime.Now;

            db.Secciones.Add(seccion);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = seccion.SeccionID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }

            //ViewBag.SectorID = new SelectList(db.Sectores, "SectorID", "Nombre");
            //return View();
        }

        // GET: Seccion/Edit/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Seccion seccion = await db.Secciones.FindAsync(id);
            if (seccion == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }

            if (seccion.Casillas != null) seccion.Casillas = seccion.Casillas.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Tipo).ToList();
            if (seccion.Colonias != null) seccion.Colonias = seccion.Colonias.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Nombre).ToList();
            ViewBag.SectorID = new SelectList(db.Sectores.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Nombre), "SectorID", "Nombre", seccion.SectorID);

            return View(seccion);
        }

        // POST: Seccion/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit([Bind(Include = "SeccionID,SectorID,Numero,Descripcion,Status,UserNameActualizacion,FechaActualizacion")] Seccion seccion)
        {
            bool esNuevo = seccion.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                seccion.Status = StatusTipo.Activo;
            }

            if (await ExisteSeccionAsync(seccion))
            {
                ModelState.AddModelError("Numero", "El número de sección ya existe, revise si esta dada de baja o eliminada.");
            }

            if (ModelState.IsValid)
            {
                seccion.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                seccion.FechaActualizacion = DateTime.Now;
                db.Entry(seccion).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }

            if (seccion.Casillas != null) seccion.Casillas = seccion.Casillas.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Tipo).ToList();
            if (seccion.Colonias != null) seccion.Colonias = seccion.Colonias.Where(c => c.Status == StatusTipo.Activo).OrderBy(c => c.Nombre).ToList();
            ViewBag.SectorID = new SelectList(db.Sectores.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Nombre), "SectorID", "Nombre", seccion.SectorID);

            return View(seccion);
        } // Edit

        // GET: Seccion/Delete/5
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
            Seccion seccion = await db.Secciones.FindAsync(id);
            if (seccion == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
            }
            return View(seccion);
        } // Delete

        // POST: Seccion/Delete/5
        [Authorize(Roles = "Admin, Editor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Seccion seccion = await db.Secciones.FindAsync(id);
            db.Secciones.Remove(seccion);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // METODOS PRIVADOS

        private async Task<bool> ExisteSeccionAsync(Seccion seccion)
        {
            var total = await (from s in db.Secciones
                               where s.Numero == seccion.Numero && s.SeccionID != seccion.SeccionID
                               select s).CountAsync();
            return (total > 0);
        } // ExisteSeccion

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var secciones = await (from s in db.Secciones
                            where s.Status == StatusTipo.Ninguno
                                && string.Compare(s.UserNameActualizacion, userName, true) == 0
                            select s).ToListAsync();

            foreach (Seccion sec in secciones)
            {
                db.Secciones.Remove(sec);
            }
            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
    }
}
