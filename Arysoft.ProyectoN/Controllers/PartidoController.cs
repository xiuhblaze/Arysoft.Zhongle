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
    public class PartidoController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Partido
        [Authorize(Roles = "Admin, Editor, Consultant")]
        public async Task<ActionResult> Index(string buscar, string filtro, string orden)
        {
            ViewBag.Orden = orden;
            ViewBag.OrdenNombre = string.IsNullOrEmpty(orden) ? "nombre_desc" : "";
            ViewBag.OrdenCandidato = orden == "candidato" ? "candidato_desc" : "candidato";
            ViewBag.OrdenVotos = orden == "votos" ? "votos_desc" : "votos";

            if (buscar != null)
            {
                // pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;

            var partidos = db.Partidos.Where(p => p.Status != StatusTipo.Ninguno);

            if (!string.IsNullOrEmpty(buscar))
            {
                partidos = partidos.Where(p => p.Nombre.Contains(buscar)
                    || p.Siglas.Contains(buscar)
                    || p.Candidato.Contains(buscar)
                );
            }

            switch (orden)
            {
                case "nombre_desc":
                    partidos = partidos.OrderByDescending(p => p.Nombre);
                    break;
                case "candidato":
                    partidos = partidos.OrderBy(p => p.Candidato);
                    break;
                case "candidato_desc":
                    partidos = partidos.OrderBy(p => p.Candidato);
                    break;
                default:
                    partidos = partidos.OrderBy(p => p.Nombre);
                    break;
            }

            return View(await partidos.ToListAsync());
        } // Index

        // GET: Partido/Details/5
        [Authorize(Roles = "Admin, Editor, Consultant")]
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

            Partido partido = await db.Partidos.FindAsync(id);

            if (partido == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador.";
                if (Request.IsAjaxRequest())
                {
                    return Content("nofound");
                }
                return RedirectToAction("Index");
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_details", partido);
            }

            return View(partido);
        } // Details

        // GET: Partido/Create
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(ControllerContext.HttpContext.User.Identity.Name);

            Partido partido = new Partido
            {
                PartidoID = Guid.NewGuid(),
                Nombre = string.Empty,
                Siglas = string.Empty,
                Candidato = string.Empty,
                Status = StatusTipo.Ninguno,
                UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name,
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

        //// POST: Partido/Create
        //// Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        //// más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "PartidoID,Nombre,Siglas,Candidato,Status")] Partido partido)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        partido.PartidoID = Guid.NewGuid();
        //        db.Partidos.Add(partido);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(partido);
        //} // Create

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
            Partido partido = await db.Partidos.FindAsync(id);
            if (partido == null)
            {
                //return HttpNotFound();
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }
            return View(partido);
        }

        // POST: Partido/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Editor")]
        public async Task<ActionResult> Edit([Bind(Include = "PartidoID,Nombre,Siglas,Candidato,Status,UserNameActualizacion,FechaActualizacion")] Partido partido)
        {
            bool esNuevo = partido.Status == StatusTipo.Ninguno;

            if (esNuevo)
            {
                partido.Status = StatusTipo.Activo;
            }

            if (ModelState.IsValid)
            {
                partido.UserNameActualizacion = ControllerContext.HttpContext.User.Identity.Name;
                partido.FechaActualizacion = DateTime.Now;

                db.Entry(partido).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }
            return View(partido);
        }

        // GET: Partido/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Partido partido = await db.Partidos.FindAsync(id);
            if (partido == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            return View(partido);
        }

        // POST: Partido/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Partido partido = await db.Partidos.FindAsync(id);
            db.Partidos.Remove(partido);
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

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var partidos = await (from p in db.Partidos
                            where p.Status == StatusTipo.Ninguno
                            && string.Compare(p.UserNameActualizacion, userName, true) == 0
                            select p).ToListAsync();

            foreach (Partido partido in partidos)
            {
                db.Partidos.Remove(partido);
            }

            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporales
    }
}
