using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Arysoft.ProyectoN.Models;

namespace Arysoft.ProyectoN.Controllers
{
    public class CandidatoController : Controller
    {
        private const string RUTA_ARCHIVOS = "~/Archivos/Candidatos";
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET: Candidato
        [Authorize(Roles = "Admin, Editor, SectorEditor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenVotos = orden == "votos" ? "votos_desc" : "votos";

            if (buscar != null)
            {
                // pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var candidatos = db.Candidatos
                .Include(c => c.Partidos)
                .Where(c => c.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar))
            {
                candidatos = candidatos.Where(c => c.Nombre.Contains(buscar)
                    || c.Descripcion.Contains(buscar)
                    || c.Coalicion.Contains(buscar)
                    || c.SiglasCoalicion.Contains(buscar)
                    || c.Partidos.Select(p => p.Nombre.Contains(buscar)).Count() > 0
                    || c.Partidos.Select(p => p.Siglas.Contains(buscar)).Count() > 0
                );
            }
            else
            {
                if (User.IsInRole("Admin"))
                {
                    candidatos = candidatos.Where(p => p.Status != StatusTipo.Ninguno);
                }
                else if (User.IsInRole("Editor") || User.IsInRole("SectorEditor"))
                {
                    candidatos = candidatos.Where(p => p.Status == StatusTipo.Activo || p.Status == StatusTipo.Baja);
                }
                else if (User.IsInRole("Consultant"))
                {
                    candidatos = candidatos.Where(p => p.Status == StatusTipo.Activo);
                }
            }

            switch (orden)
            {
                case "nombre_desc":
                    candidatos = candidatos.OrderByDescending(p => p.Nombre);
                    break;
                case "votos":
                    candidatos = candidatos.OrderBy(p => p.VotosTotal);
                    break;
                case "votos_desc":
                    candidatos = candidatos.OrderBy(p => p.VotosTotal);
                    break;
                default:
                    candidatos = candidatos.OrderBy(p => p.Nombre);
                    break;
            }

            return View(await candidatos.ToListAsync());
        } // Index

        // GET: Candidato/Details/5
        [Authorize(Roles = "Admin, Editor, SectorEditor, Consultant")]
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                if (Request.IsAjaxRequest()) { return Content("noid"); }
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Candidato candidato = await db.Candidatos
                .Include(c => c.Partidos)
                .FirstOrDefaultAsync(c => c.CandidatoID == id);
            if (candidato == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest()) { return Content("nofound"); }
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            candidato.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                candidato.Origen = "details";
                return PartialView("_details", candidato);
            }
            return View(candidato);
        } // Details

        // GET: Candidato/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(User.Identity.Name);

            Candidato candidato = new Candidato
            {
                CandidatoID = Guid.NewGuid(),
                Nombre = string.Empty,
                VotosTotal = 0,
                Campanna = CampannaTipo.Ninguno,
                Tipo = CandidatoTipo.Ninguno,
                Status = StatusTipo.Ninguno,
                UserNameActualizacion = User.Identity.Name,
                FechaActualizacion = DateTime.Now
            };
            db.Candidatos.Add(candidato);
            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = candidato.CandidatoID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
        } // Create

        // GET: Candidato/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Candidato candidato = await db.Candidatos
                .Include(c => c.Partidos)
                .FirstOrDefaultAsync(c => c.CandidatoID == id);
            if (candidato == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }
            candidato.SoloLectura = false;
            return View(candidato);
        } // GET: Edit

        // POST: Candidato/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "CandidatoID,Nombre,Descripcion,ArchivoFotografia,VotosTotal,Campanna,Tipo,Coalicion,SiglasCoalicion,ArchivoCoalicion,Status,UserNameActualizacion,FechaActualizacion")] Candidato candidato, HttpPostedFileBase archivo, HttpPostedFileBase archivoCoalicion)
        {
            bool esNuevo = candidato.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                candidato.Status = StatusTipo.Activo;
            }

            if (ModelState.IsValid)
            {
                if (archivo != null)
                {
                    candidato.ArchivoFotografia = GuardarArchivo(archivo, candidato.CandidatoID.ToString(), "fotografia");
                }
                if (archivoCoalicion != null)
                {
                    candidato.ArchivoCoalicion = GuardarArchivo(archivo, candidato.CandidatoID.ToString(), "coalicion");
                }
                candidato.UserNameActualizacion = User.Identity.Name;
                candidato.FechaActualizacion = DateTime.Now;
                db.Entry(candidato).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }
                return RedirectToAction("Index");
            }
            candidato.SoloLectura = false;
            return View(candidato);
        } // POST: Edit

        // GET: Candidato/Delete/5
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
            Candidato candidato = await db.Candidatos
                .Include(c => c.Partidos)
                .FirstOrDefaultAsync(c => c.CandidatoID == id);
            if (candidato == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                if (Request.IsAjaxRequest()) { return Content("notfound"); }
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            candidato.SoloLectura = true;
            if (Request.IsAjaxRequest())
            {
                candidato.Origen = "delete";
                return PartialView("_details", candidato);
            }
            return View(candidato);
        } // GET: Delete

        // POST: Candidato/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Candidato candidato = await db.Candidatos.FindAsync(id);

            if (candidato.Status == StatusTipo.Activo)
            {
                candidato.Status = StatusTipo.Baja;
                candidato.UserNameActualizacion = User.Identity.Name;
                candidato.FechaActualizacion = DateTime.Now;
                db.Entry(candidato).State = EntityState.Modified;
            }
            else if (candidato.Status == StatusTipo.Baja)
            {
                candidato.Status = StatusTipo.Eliminado;
                candidato.UserNameActualizacion = User.Identity.Name;
                candidato.FechaActualizacion = DateTime.Now;
                db.Entry(candidato).State = EntityState.Modified;
            }
            else
            {
                var path = Path.Combine(Server.MapPath(RUTA_ARCHIVOS), candidato.CandidatoID.ToString());
                if (Directory.Exists(path)) { Directory.Delete(path, true); }
                db.Candidatos.Remove(candidato);
            }            
            await db.SaveChangesAsync();
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

        private string GuardarArchivo(HttpPostedFileBase control, string carpeta, string nombre)
        {
            var path = Path.Combine(Server.MapPath(RUTA_ARCHIVOS), carpeta);
            var extencion = Path.GetExtension(control.FileName).ToLower();

            if (string.IsNullOrEmpty(nombre)) nombre = Path.GetFileNameWithoutExtension(control.FileName);
            nombre = nombre.CleanInvalidFileNameChars();
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }

            control.SaveAs(Path.Combine(path, nombre + extencion));

            return nombre + extencion;
        } // GuardarArchivo

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var candidatos = await (from c in db.Candidatos
                                    where c.Status == StatusTipo.Ninguno
                                        && string.Compare(c.UserNameActualizacion, userName, true) == 0
                                    select c).ToListAsync();
            foreach (Candidato candidato in candidatos)
            {
                var path = Path.Combine(Server.MapPath(RUTA_ARCHIVOS), candidato.CandidatoID.ToString());

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                db.Candidatos.Remove(candidato);
            }
            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporalesAsync
    }
}
