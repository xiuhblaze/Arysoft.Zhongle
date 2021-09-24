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
    public class PaginasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Paginas
        public async Task<ActionResult> Index()
        {
            var paginas = db.Paginas.Include(p => p.PaginaPadre);
            return View(await paginas.ToListAsync());
        }

        // GET: Admin/Paginas/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagina pagina = await db.Paginas.FindAsync(id);
            if (pagina == null)
            {
                return HttpNotFound();
            }
            return View(pagina);
        }

        // GET: Admin/Paginas/Create
        public ActionResult Create()
        {
            ViewBag.PaginaPadreID = new SelectList(db.Paginas, "PaginaID", "Titulo");
            return View();
        }

        // POST: Admin/Paginas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PaginaID,PaginaPadreID,Titulo,IndiceMenu,EtiquetaMenu,Resumen,HTMLContent,FriendlyUrl,TargetUrl,Target,TieneGaleria,ContadorVisitas,FechaContador,Idioma,EsPrincipal,HTMLHeadScript,HTMLFooterScript,MeGusta,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Pagina pagina)
        {
            if (ModelState.IsValid)
            {
                pagina.PaginaID = Guid.NewGuid();
                db.Paginas.Add(pagina);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.PaginaPadreID = new SelectList(db.Paginas, "PaginaID", "Titulo", pagina.PaginaPadreID);
            return View(pagina);
        }

        // GET: Admin/Paginas/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagina pagina = await db.Paginas.FindAsync(id);
            if (pagina == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaginaPadreID = new SelectList(db.Paginas, "PaginaID", "Titulo", pagina.PaginaPadreID);
            return View(pagina);
        }

        // POST: Admin/Paginas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PaginaID,PaginaPadreID,Titulo,IndiceMenu,EtiquetaMenu,Resumen,HTMLContent,FriendlyUrl,TargetUrl,Target,TieneGaleria,ContadorVisitas,FechaContador,Idioma,EsPrincipal,HTMLHeadScript,HTMLFooterScript,MeGusta,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Pagina pagina)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pagina).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.PaginaPadreID = new SelectList(db.Paginas, "PaginaID", "Titulo", pagina.PaginaPadreID);
            return View(pagina);
        }

        // GET: Admin/Paginas/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagina pagina = await db.Paginas.FindAsync(id);
            if (pagina == null)
            {
                return HttpNotFound();
            }
            return View(pagina);
        }

        // POST: Admin/Paginas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Pagina pagina = await db.Paginas.FindAsync(id);
            db.Paginas.Remove(pagina);
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
    }
}
