using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Arysoft.ProyectoN.Models;

namespace Arysoft.ProyectoN.Controllers
{
    [Authorize]
    public class PartidoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Partido
        [Authorize(Roles = "Admin, Editor, SectorEditor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenSiglas = orden == "siglas" ? "siglas_desc" : "siglas";

            if (buscar != null)
            {
                // pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var partidos = db.Partidos
                .Where(p => p.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar))
            {
                partidos = partidos.Where(p => p.Nombre.Contains(buscar)
                    || p.Siglas.Contains(buscar)
                    || p.Candidatos.Select(c => c.Nombre.Contains(buscar)).Count() > 0
                );
            }
            else 
            {
                if (User.IsInRole("Admin"))
                {
                    partidos = partidos.Where(p => p.Status != StatusTipo.Ninguno);
                }
                else if (User.IsInRole("Editor") || User.IsInRole("SectorEditor"))
                {
                    partidos = partidos.Where(p => p.Status == StatusTipo.Activo || p.Status == StatusTipo.Baja);
                }
                else if (User.IsInRole("Consultant"))
                {
                    partidos = partidos.Where(p => p.Status == StatusTipo.Activo);
                }
            }

            switch (orden)
            {
                case "nombre_desc":
                    partidos = partidos.OrderByDescending(p => p.Nombre);
                    break;
                case "siglas":
                    partidos = partidos.OrderBy(p => p.Siglas);
                    break;
                case "siglas_desc":
                    partidos = partidos.OrderBy(p => p.Siglas);
                    break;
                default:
                    partidos = partidos.OrderBy(p => p.Nombre);
                    break;
            }

            return View(await partidos.ToListAsync());
        } // Index

        // GET: Partido/Details/5
        [Authorize(Roles = "Admin, Editor, SectorEditor, Consultant")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("noid"); }
                return RedirectToAction("Index");
            }
            Partido partido = await db.Partidos
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p => p.PartidoID == id);
            if (partido == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("nofound"); }
                return RedirectToAction("Index");
            }

            partido.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                partido.Origen = "details";
                return PartialView("_details", partido);
            }

            return View(partido);
        } // Details

        // GET: Partido/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(User.Identity.Name);

            Partido partido = new Partido
            {
                PartidoID = Guid.NewGuid(),
                Nombre = string.Empty,
                Siglas = string.Empty,
                ArchivoLogotipo = string.Empty,
                Status = StatusTipo.Ninguno,
                UserNameActualizacion = User.Identity.Name,
                FechaActualizacion = DateTime.Now
            };

            db.Partidos.Add(partido);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = partido.PartidoID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
        } // Create

        // GET: Partido/Edit/5
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Partido partido = await db.Partidos
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p => p.PartidoID == id);
            if (partido == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }
            partido.SoloLectura = false;
            return View(partido);
        } // Edit

        // POST: Partido/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit([Bind(Include = "PartidoID,Nombre,Siglas,ArchivoLogotipo,Status,UserNameActualizacion,FechaActualizacion")] Partido partido, HttpPostedFileBase archivo)
        {
            bool esNuevo = partido.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                partido.Status = StatusTipo.Activo;
            }

            if (ModelState.IsValid)
            {
                if (archivo != null)
                {
                    partido.ArchivoLogotipo = GuardarArchivo(archivo, partido.PartidoID.ToString(), "logotipo");
                }
                partido.UserNameActualizacion = User.Identity.Name;
                partido.FechaActualizacion = DateTime.Now;

                db.Entry(partido).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }
            partido.SoloLectura = false;

            return View(partido);
        } // Edit

        // GET: Partido/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("notid"); }
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Partido partido = await db.Partidos
                .Include(p => p.Candidatos)
                .FirstOrDefaultAsync(p => p.PartidoID == id);
            if (partido == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            partido.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                partido.Origen = "delete";
                return PartialView("_details", partido);
            }
            return View(partido);
        } // Delete

        // POST: Partido/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Partido partido = await db.Partidos.FindAsync(id);

            if (partido.Status == StatusTipo.Activo)
            {
                partido.Status = StatusTipo.Baja;
                partido.UserNameActualizacion = User.Identity.Name;
                partido.FechaActualizacion = DateTime.Now;
                db.Entry(partido).State = EntityState.Modified;
            }
            else if (partido.Status == StatusTipo.Baja)
            {
                partido.Status = StatusTipo.Eliminado;
                partido.UserNameActualizacion = User.Identity.Name;
                partido.FechaActualizacion = DateTime.Now;
                db.Entry(partido).State = EntityState.Modified;
            }
            else
            {
                var path = Path.Combine(Server.MapPath("~/Archivos/Partidos"), partido.PartidoID.ToString());
                if (Directory.Exists(path)) { Directory.Delete(path, true); }
                db.Partidos.Remove(partido);
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        } // DeleteConfirmed

        // POST: Calle/Activar/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activar(Guid id)
        {
            Partido partido = await db.Partidos.FindAsync(id);
            if (partido == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                return RedirectToAction("Index");
            }
            partido.Status = StatusTipo.Activo;
            partido.UserNameActualizacion = User.Identity.Name;
            partido.FechaActualizacion = DateTime.Now;
            db.Entry(partido).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "El partido ha sido reactivado.";

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

        // METODOS PRIVADOS

        private string GuardarArchivo(HttpPostedFileBase control, string carpeta, string nombre)
        {   
            var path = Path.Combine(Server.MapPath("~/Archivos/Partidos"), carpeta);
            var extencion = Path.GetExtension(control.FileName).ToLower();

            if (string.IsNullOrEmpty(nombre)) nombre = Path.GetFileNameWithoutExtension(control.FileName);
            nombre = nombre.CleanInvalidFileNameChars();
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

            control.SaveAs(Path.Combine(path, nombre + extencion));

            return nombre + extencion;
        } // GuardarArchivo

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var partidos = await (from p in db.Partidos
                            where p.Status == StatusTipo.Ninguno
                            && string.Compare(p.UserNameActualizacion, userName, true) == 0
                            select p).ToListAsync();
            foreach (Partido partido in partidos)
            {
                var path = Path.Combine(Server.MapPath("~/Archivos/Partidos"), partido.PartidoID.ToString());

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                db.Partidos.Remove(partido);
            }

            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
    }
}