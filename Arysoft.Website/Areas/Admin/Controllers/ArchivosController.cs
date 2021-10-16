using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Arysoft.Website.Models;
using Arysoft.Website.Areas.Admin.Models;

namespace Arysoft.Website.Areas.Admin.Controllers
{
    public class ArchivosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Archivos
        public async Task<ActionResult> Index()
        {
            return View(await db.Archivos.ToListAsync());
        }

        // GET: Admin/Archivos/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Archivo archivo = await db.Archivos.FindAsync(id);
            if (archivo == null)
            {
                return HttpNotFound();
            }
            return View(archivo);
        }

        // GET: Admin/Archivos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Archivos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ArchivoID,PropietarioID,Indice,Nombre,Descripcion,Tags,EnGaleria,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Archivo archivo)
        {
            if (ModelState.IsValid)
            {
                archivo.ArchivoID = Guid.NewGuid();
                db.Archivos.Add(archivo);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(archivo);
        }

        // GET: Admin/Archivos/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Archivo archivo = await db.Archivos.FindAsync(id);
            if (archivo == null)
            {
                return HttpNotFound();
            }
            return View(archivo);
        }

        // POST: Admin/Archivos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ArchivoID,PropietarioID,Indice,Nombre,Descripcion,Tags,EnGaleria,Status,FechaCreacion,FechaActualizacion,UsuarioActualizacion")] Archivo archivo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(archivo).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(archivo);
        }

        // GET: Admin/Archivos/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Archivo archivo = await db.Archivos.FindAsync(id);
            if (archivo == null)
            {
                return HttpNotFound();
            }
            return View(archivo);
        }

        // POST: Admin/Archivos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Archivo archivo = await db.Archivos.FindAsync(id);
            db.Archivos.Remove(archivo);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AgregarArchivos(Guid id)
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    string carpeta = Request.Params["isPage"] == "true" ? "Paginas/" : "Noticias/";
                    string dname = Path.Combine(Server.MapPath("~/Archivos"), carpeta, id.ToString());
                    string fname = string.Empty;
                    string fextension = string.Empty;

                    BoolTipo incluirGaleria = Request.Params["incluirGaleria"] == "true" ? BoolTipo.Si : BoolTipo.No;
                    //string[] allowedExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
                    //string[] imagesExtensions = new string[] { ".jpg", ".gif", ".png" };

                    for (int i = 0; i < files.Count; i++)
                    {
                        BoolTipo enGaleria = BoolTipo.No;
                        HttpPostedFileBase file = files[i];

                        fname = Path.GetFileNameWithoutExtension(file.FileName);
                        fname = fname.ToSingleSpaces().CleanInvalidFileNameChars();
                        fextension = Path.GetExtension(file.FileName).ToLower();

                        if (!Array.Exists(Areas.Admin.Models.Comun.FILES_ALLOWED_EXTENSIONS, e => e == fextension)) { continue; }
                        if (Array.Exists(Areas.Admin.Models.Comun.FILES_IMAGES_EXTENSIONS, e => e == fextension)
                            && incluirGaleria == BoolTipo.Si)
                        {
                            enGaleria = BoolTipo.Si;
                        }
                        if (!Directory.Exists(dname)) { Directory.CreateDirectory(dname); }
                        file.SaveAs(Path.Combine(dname, fname + fextension));

                        Archivo miArchivo = new Archivo()
                        {
                            ArchivoID = Guid.NewGuid(),
                            PropietarioID = id,
                            Indice = 0,
                            Nombre = fname + fextension,
                            EnGaleria = enGaleria,
                            Status = StatusTipo.Activo,
                            FechaCreacion = DateTime.Now,
                            FechaActualizacion = DateTime.Now,
                            UsuarioActualizacion = User.Identity.Name
                        };
                        db.Archivos.Add(miArchivo);
                    }

                    db.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        TempData["isReadonly"] = false;
                        TempData["isPage"] = Request.Params["isPage"] == "true";
                        var archivos = from a in db.Archivos
                                       where a.PropietarioID == id
                                       orderby a.Nombre
                                       select a;
                        return PartialView("~/Areas/Admin/Views/Archivos/_archivosList.cshtml", archivos);
                    }

                    return Json(new
                    {
                        status = "unknow",
                        message = "Esto no deberia llegar hasta aqui (por ahora)."
                    });
                }
                catch (Exception e)
                {
                    return Json(new
                    {
                        status = "exception",
                        message = "A ocurrido una excepción: " + e.Message
                    });
                }
            }
            else
            {
                return Json(new
                {
                    status = "nofile",
                    message = "No se recibió ningun archivo."
                });
            }
        } // AgregarArchivos

        [HttpPost]
        public ActionResult EliminarArchivo(Guid id)
        {
            var archivo = db.Archivos.Find(id);

            if (archivo == null)
            {
                return Json(new
                {
                    status = "HttpNotFound",
                    message = "No se encontró el registro del archivo."
                });
            }

            Guid propietarioID = archivo.PropietarioID;
            Pagina pagina = db.Paginas.Find(propietarioID);
            string carpeta = pagina != null ? "Paginas/" : "Noticias/";

            string dname = Path.Combine(Server.MapPath("~/Archivos"), carpeta, propietarioID.ToString());
            string fname = archivo.Nombre;
            string path = Path.Combine(dname, fname);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            db.Archivos.Remove(archivo);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = false;
                TempData["isPage"] = pagina != null;
                var archivos = from a in db.Archivos
                               where a.PropietarioID == propietarioID
                               orderby a.Nombre
                               select a;
                return PartialView("~/Areas/Admin/Views/Archivos/_archivosList.cshtml", archivos);
            }

            return Json(new
            {
                status = "unknow",
                message = "Esto no deberia llegar hasta aqui (por ahora)."
            });
        } // EliminarArchivo

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
