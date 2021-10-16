using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Arysoft.Website.Models;

namespace Arysoft.Website.Areas.Admin.Controllers
{
    public class NotasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Notas
        public async Task<ActionResult> Index()
        {
            return View(await db.Notas.ToListAsync());
        }

        // GET: Admin/Notas/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nota nota = await db.Notas.FindAsync(id);
            if (nota == null)
            {
                return HttpNotFound();
            }
            return View(nota);
        }

        // GET: Admin/Notas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Notas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "NotaID,PropietarioID,Texto,Autor,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Nota nota)
        {
            if (ModelState.IsValid)
            {
                nota.NotaID = Guid.NewGuid();
                db.Notas.Add(nota);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(nota);
        }

        // GET: Admin/Notas/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nota nota = await db.Notas.FindAsync(id);
            if (nota == null)
            {
                return HttpNotFound();
            }
            return View(nota);
        }

        // POST: Admin/Notas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "NotaID,PropietarioID,Texto,Autor,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Nota nota)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nota).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(nota);
        }

        // GET: Admin/Notas/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Nota nota = await db.Notas.FindAsync(id);
            if (nota == null)
            {
                return HttpNotFound();
            }
            return View(nota);
        }

        // POST: Admin/Notas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Nota nota = await db.Notas.FindAsync(id);
            db.Notas.Remove(nota);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AgregarNota(Guid id, string nota)
        {
            if (string.IsNullOrEmpty(nota))
            {
                return Json(new
                {
                    status = "notnote",
                    message = "No se encontró la nota."
                });
            }

            try
            {
                Nota miNota = new Nota()
                {
                    NotaID = Guid.NewGuid(),
                    PropietarioID = id,
                    Texto = nota,
                    Autor = User.Identity.Name,
                    Status = StatusTipo.Activo,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    UsuarioActualizacion = User.Identity.Name
                };
                db.Notas.Add(miNota);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new
                {
                    status = "exception",
                    message = "A ocurrido una excepción: " + e.Message
                });
            }

            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = false;
                TempData["isPage"] = false;
                var notas = from n in db.Notas
                            where n.PropietarioID == id
                            orderby n.FechaCreacion descending
                            select n;
                return PartialView("~/Areas/Admin/Views/Notas/_notasList.cshtml", notas);
            }

            return Json(new
            {
                status = "unknow",
                message = "Esto no deberia llegar hasta aqui (por ahora)."
            });
        } // AgregarNota

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
