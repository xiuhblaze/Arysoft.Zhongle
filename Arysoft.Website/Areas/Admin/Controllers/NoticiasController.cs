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
using PagedList;

namespace Arysoft.Website.Areas.Admin.Controllers
{
    [Authorize]
    public class NoticiasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Noticias
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string status, int? pagina)
        {
            NoticiaStatus myStatus = (NoticiaStatus)(string.IsNullOrEmpty(status) ? NoticiaStatus.Ninguno : Enum.Parse(typeof(NoticiaStatus), status));

            ViewBag.MenuActive = "noticias";

            ViewBag.Orden = orden;
            ViewBag.OrdenPublicacion = string.IsNullOrEmpty(orden) ? "publicacion" : "";
            ViewBag.OrdenNombre = orden == "titulo" ? "titulo_desc" : "";
            ViewBag.OrdenAutor = orden == "autor" ? "autor_desc" : "autor";
            ViewBag.OrdenVisitas = orden == "visitas" ? "visitas_desc" : "visitas";
            ViewBag.OrdenMeGusta = orden == "megusta" ? "megusta_desc" : "megusta";
            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;
            var noticias = db.Noticias
                .Include(n => n.Archivos)
                .Include(n => n.Notas)
                .Where(n => n.Status != NoticiaStatus.Ninguno);
            if (!string.IsNullOrEmpty(buscar) || myStatus != NoticiaStatus.Ninguno)
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);

                // status = string.IsNullOrEmpty(status) ? NoticiaStatus.Ninguno.ToString() : status;
                noticias = noticias.Where(n =>
                    (
                        (!hayPalabras)
                        || n.Titulo.Contains(buscar)
                        || n.Resumen.Contains(buscar)
                        || n.HTMLContent.Contains(buscar)
                    )
                    && (myStatus == NoticiaStatus.Ninguno ? true : n.Status == myStatus)
                );
            }
            switch (orden)
            {
                case "publicacion":
                    noticias = noticias.OrderBy(n => n.FechaPublicacion);
                    break;
                case "titulo":
                    noticias = noticias.OrderBy(n => n.Titulo);
                    break;
                case "titulo_desc":
                    noticias = noticias.OrderByDescending(n => n.Titulo);
                    break;
                case "autor":
                    noticias = noticias.OrderBy(n => n.Autor);
                    break;
                case "autor_desc":
                    noticias = noticias.OrderByDescending(n => n.Autor);
                    break;
                case "visitas":
                    noticias = noticias.OrderBy(n => n.ContadorVisitas);
                    break;
                case "visitas_desc":
                    noticias = noticias.OrderByDescending(n => n.ContadorVisitas);
                    break;
                case "megusta":
                    noticias = noticias.OrderBy(n => n.MeGusta);
                    break;
                case "megusta_desc":
                    noticias = noticias.OrderByDescending(n => n.MeGusta);
                    break;
                default:
                    noticias = noticias.OrderByDescending(n => n.FechaPublicacion);
                    break;
            }
            List<NoticiaIndexListViewModel> noticiasViewModel = new List<NoticiaIndexListViewModel>();
            foreach (Noticia noticia in await noticias.ToListAsync())
            {
                noticiasViewModel.Add(new NoticiaIndexListViewModel(noticia));
            }
            ViewBag.Count = noticiasViewModel.Count();
            int numeroPagina = pagina ?? 1;
            int elementosPagina = Comun.ELEMENTOS_PAGINA;

            return View(noticiasViewModel.ToPagedList(numeroPagina, elementosPagina));
        } // Index

        // GET: Admin/Noticias/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador";
                if (Request.IsAjaxRequest()) return Content("BadRequest");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Noticia noticia = await db.Noticias
                .Include(n => n.Archivos)
                .Include(n => n.Notas)
                .FirstOrDefaultAsync(n => n.NoticiaID == id);
            if (noticia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }

            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = true;
                TempData["isPage"] = false;
                NoticiaDetailsViewModel n = new NoticiaDetailsViewModel(noticia, "details", true);
                return PartialView("_details", n);
            }
            return View(noticia);
        } // Details

        // GET: Admin/Noticias/Create
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(User.Identity.Name);

            Noticia noticia = new Noticia
            {
                NoticiaID = Guid.NewGuid(),
                Titulo = string.Empty,
                HTMLContent = string.Empty,
                TieneGaleria = BoolTipo.Ninguno,
                ContadorVisitas = 0,
                Idioma = IdiomaTipo.Ninguno,
                FechaPublicacion = DateTime.Now,
                MeGusta = 0,
                Status = NoticiaStatus.Ninguno,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                UsuarioActualizacion = User.Identity.Name
            };

            db.Noticias.Add(noticia);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = noticia.NoticiaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
        } // Create

        // GET: Admin/Noticias/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Noticia noticia = await db.Noticias
                .Include(n => n.Archivos)
                .Include(n => n.Notas)
                .FirstOrDefaultAsync(n => n.NoticiaID == id);
            if (noticia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            if (noticia.Status == NoticiaStatus.Eliminada)
            {
                TempData["MessageBox"] = "La noticia ha sido eliminada.";
                return RedirectToAction("Index");
            }

            TempData["isReadonly"] = false;
            TempData["isPage"] = false;
            return View(new NoticiaEditViewModel(noticia));
        } // Edit

        // POST: Admin/Noticias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "NoticiaID,Titulo,__resumen,__content,FriendlyUrl,Autor,TieneGaleria,ContadorVisitas,Idioma,FechaPublicacion,ImagenArchivo,MeGusta,Status,FechaCreacion")] NoticiaEditViewModel noticiaVM)
        {
            Noticia noticia = noticiaVM.ObtenerNoticia();

            bool esNuevo = noticia.Status == NoticiaStatus.Ninguno;

            if (esNuevo)
            {
                noticia.Status = NoticiaStatus.Nueva;
            }

            if (ModelState.IsValid)
            {
                noticia.FechaActualizacion = DateTime.Now;
                noticia.UsuarioActualizacion = User.Identity.Name;
                db.Entry(noticia).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }

                return RedirectToAction("Index");
            }

            TempData["isReadonly"] = false;
            TempData["isPage"] = false;
            return View(noticia);
        } // Edit

        // GET: Admin/Noticias/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador";
                if (Request.IsAjaxRequest()) return Content("BadRequest");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Noticia noticia = await db.Noticias
                .Include(n => n.Archivos)
                .Include(n => n.Notas)
                .FirstOrDefaultAsync(n => n.NoticiaID == id);
            if (noticia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }

            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = true;
                TempData["isPage"] = false;
                NoticiaDetailsViewModel n = new NoticiaDetailsViewModel(noticia, "delete", true);
                return PartialView("_details", n);
            }
            return View(noticia);
        } // Delete

        // POST: Admin/Noticias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Noticia noticia = await db.Noticias.FindAsync(id);

            if (noticia.Status == NoticiaStatus.Eliminada)
            {
                // Eliminando la carpeta de archivos de la noticia
                string dname = Path.Combine(Server.MapPath("~/Archivos/Noticias/"), noticia.NoticiaID.ToString());
                if (Directory.Exists(dname)) { Directory.Delete(dname, true); }
                // Eliminando registros asociados a la noticia
                foreach (Nota n in await db.Notas.Where(n => n.PropietarioID == noticia.NoticiaID).ToListAsync())
                {
                    db.Notas.Remove(n);
                }
                foreach (Archivo a in await db.Archivos.Where(a => a.PropietarioID == noticia.NoticiaID).ToListAsync())
                {
                    db.Archivos.Remove(a);
                }
                // Eliminando la noticia
                db.Noticias.Remove(noticia);
            }
            else
            {
                noticia.Status = NoticiaStatus.Eliminada;
                noticia.FechaActualizacion = DateTime.Now;
                noticia.UsuarioActualizacion = User.Identity.Name;
                db.Entry(noticia).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        } // DeleteConfirmed

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activar(Guid id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador";
                if (Request.IsAjaxRequest()) return Content("BadRequest");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Noticia noticia = await db.Noticias.FindAsync(id);
            if (noticia == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }
            noticia.Status = NoticiaStatus.Nueva;
            noticia.FechaActualizacion = DateTime.Now;
            noticia.UsuarioActualizacion = User.Identity.Name;
            db.Entry(noticia).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "La noticia ha sido reactivada.";

            return RedirectToAction("Index");
        } // Activar

        [HttpPost]
        public ActionResult AgregarImagenPrincipal(Guid id)
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    // Get all files from Request object
                    HttpFileCollectionBase files = Request.Files;
                    string dname = Path.Combine(Server.MapPath("~/Archivos/Noticias/"), id.ToString());
                    string fname = id.ToString();
                    string fextension = string.Empty;

                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];

                        fextension = Path.GetExtension(file.FileName).ToLower();
                        // Get the complete folder path and store the file inside it.                        
                        fname += fextension;
                        if (!Directory.Exists(dname)) { Directory.CreateDirectory(dname); }
                        file.SaveAs(Path.Combine(dname, fname));
                    }

                    return Json(new
                    {
                        status = "success",
                        message = "File Uploaded Successfully!",
                        filename = fname
                    });
                }
                catch (Exception e)
                {
                    return Json(new
                    {
                        status = "exception",
                        message = "A ocurrido una excepción: " + e.Message,
                        filename = ""
                    });
                }
            }
            else
            {
                return Json(new
                {
                    status = "exception",
                    message = "No se seleccionó ningún archivo.",
                    filename = ""
                });
            }
        } // AgregarImagenPrincipal

        /// <summary>
        /// Obtiene una cadena de texto y la convierte en una url friendly, evitando existentes
        /// y evitanto compararla con la del ID que la mando generar.
        /// </summary>
        /// <param name="id">Identificador de la noticia que mando generar la url</param>
        /// <param name="valor">Cadena de texto que recibe para ser convertida</param>
        /// <returns></returns>
        public async Task<ActionResult> FriendlyUrl(Guid id, string valor)
        {
            string friendlyUrl = valor.ToFriendlyUrl();
            int extension = 1;
            int count = 0;
            string url = friendlyUrl;

            do
            {
                count = await db.Noticias
                    .Where(p => p.FriendlyUrl == url
                        && p.NoticiaID != id)
                    .CountAsync();
                if (count > 0)
                {
                    url = friendlyUrl + "-" + extension++.ToString("D2");
                }
            } while (count > 0);

            return Json(new
            {
                friendlyUrl = url
            });
        } // UrlFriendly

        // ARCHIVOS

        //public ActionResult AgregarArchivos(Guid id)
        //{
        //    if (Request.Files.Count > 0)
        //    {
        //        try
        //        {
        //            HttpFileCollectionBase files = Request.Files;
        //            string dname = Path.Combine(Server.MapPath("~/Archivos/Noticias/"), id.ToString());
        //            string fname = string.Empty;
        //            string fextension = string.Empty;

        //            BoolTipo incluirGaleria = Request.Params["incluirGaleria"] == "true" ? BoolTipo.Si : BoolTipo.No;
        //            //string[] allowedExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        //            //string[] imagesExtensions = new string[] { ".jpg", ".gif", ".png" };

        //            for (int i = 0; i < files.Count; i++)
        //            {
        //                BoolTipo enGaleria = BoolTipo.No;
        //                HttpPostedFileBase file = files[i];

        //                fname = Path.GetFileNameWithoutExtension(file.FileName);
        //                fname = fname.ToSingleSpaces().CleanInvalidFileNameChars();
        //                fextension = Path.GetExtension(file.FileName).ToLower();

        //                if (!Array.Exists(Areas.Admin.Models.Comun.FILES_ALLOWED_EXTENSIONS, e => e == fextension)) { continue; }
        //                if (Array.Exists(Areas.Admin.Models.Comun.FILES_IMAGES_EXTENSIONS, e => e == fextension) && incluirGaleria == BoolTipo.Si)
        //                {
        //                    enGaleria = BoolTipo.Si;
        //                }
        //                if (!Directory.Exists(dname)) { Directory.CreateDirectory(dname); }
        //                file.SaveAs(Path.Combine(dname, fname + fextension));

        //                Archivo miArchivo = new Archivo()
        //                {
        //                    ArchivoID = Guid.NewGuid(),
        //                    PropietarioID = id,
        //                    Indice = 0,
        //                    Nombre = fname + fextension,
        //                    EnGaleria = enGaleria,
        //                    Status = StatusTipo.Activo,
        //                    FechaCreacion = DateTime.Now,
        //                    FechaActualizacion = DateTime.Now,
        //                    UsuarioActualizacion = User.Identity.Name
        //                };
        //                db.Archivos.Add(miArchivo);
        //            }

        //            db.SaveChanges();

        //            if (Request.IsAjaxRequest())
        //            {
        //                TempData["isReadonly"] = false;
        //                TempData["isPage"] = false;
        //                var archivos = from a in db.Archivos
        //                               where a.PropietarioID == id
        //                               orderby a.Nombre
        //                               select a;
        //                return PartialView("~/Areas/Admin/Views/Archivos/_archivosList.cshtml", archivos);
        //            }

        //            return Json(new
        //            {
        //                status = "unknow",
        //                message = "Esto no deberia llegar hasta aqui (por ahora)."
        //            });
        //        }
        //        catch (Exception e)
        //        {
        //            return Json(new
        //            {
        //                status = "exception",
        //                message = "A ocurrido una excepción: " + e.Message
        //            });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new
        //        {
        //            status = "nofile",
        //            message = "No se recibió ningun archivo."
        //        });
        //    }
        //} // AgregarArchivos

        //[HttpPost]
        //public ActionResult EliminarArchivo(Guid id)
        //{
        //    var archivo = db.Archivos.Find(id);

        //    if (archivo == null)
        //    {
        //        return Json(new
        //        {
        //            status = "HttpNotFound",
        //            message = "No se encontró el registro del archivo."
        //        });
        //    }

        //    Guid propietarioID = archivo.PropietarioID;
        //    string dname = Path.Combine(Server.MapPath("~/Archivos/Noticias/"), propietarioID.ToString());
        //    string fname = archivo.Nombre;
        //    string path = Path.Combine(dname, fname);

        //    if (System.IO.File.Exists(path))
        //    {
        //        System.IO.File.Delete(path);
        //    }

        //    db.Archivos.Remove(archivo);
        //    db.SaveChanges();

        //    if (Request.IsAjaxRequest())
        //    {
        //        TempData["isReadonly"] = false;
        //        TempData["isPage"] = false;
        //        var archivos = from a in db.Archivos
        //                       where a.PropietarioID == propietarioID
        //                       orderby a.Nombre
        //                       select a;
        //        return PartialView("~/Areas/Admin/Views/Archivos/_archivosList.cshtml", archivos);
        //    }

        //    return Json(new
        //    {
        //        status = "unknow",
        //        message = "Esto no deberia llegar hasta aqui (por ahora)."
        //    });
        //} // EliminarArchivo

        //// NOTAS

        //[HttpPost]
        //public ActionResult AgregarNota(Guid id, string nota)
        //{
        //    if (string.IsNullOrEmpty(nota))
        //    {
        //        return Json(new
        //        {
        //            status = "notnote",
        //            message = "No se encontró la nota."
        //        });
        //    }

        //    try
        //    {
        //        Nota miNota = new Nota()
        //        {
        //            NotaID = Guid.NewGuid(),
        //            PropietarioID = id,
        //            Texto = nota,
        //            Autor = User.Identity.Name,
        //            Status = StatusTipo.Activo,
        //            FechaCreacion = DateTime.Now,
        //            FechaActualizacion = DateTime.Now,
        //            UsuarioActualizacion = User.Identity.Name
        //        };
        //        db.Notas.Add(miNota);
        //        db.SaveChanges();
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new
        //        {
        //            status = "exception",
        //            message = "A ocurrido una excepción: " + e.Message
        //        });
        //    }

        //    if (Request.IsAjaxRequest())
        //    {
        //        TempData["isReadonly"] = false;
        //        TempData["isPage"] = false;
        //        var notas = from n in db.Notas
        //                    where n.PropietarioID == id
        //                    orderby n.FechaCreacion descending
        //                    select n;
        //        return PartialView("~/Areas/Admin/Views/Notas/_notasList.cshtml", notas);
        //    }

        //    return Json(new
        //    {
        //        status = "unknow",
        //        message = "Esto no deberia llegar hasta aqui (por ahora)."
        //    });
        //} // AgregarNota

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
            var noticias = await (from n in db.Noticias
                                  where n.Status == NoticiaStatus.Ninguno
                                  && string.Compare(n.UsuarioActualizacion, userName, true) == 0
                                  select n).ToListAsync();

            foreach (var noticia in noticias)
            {
                string dname = Path.Combine(Server.MapPath("~/Archivos/Noticias/"), noticia.NoticiaID.ToString());
                if (Directory.Exists(dname)) { Directory.Delete(dname, true); }

                foreach (Nota n in await db.Notas.Where(n => n.PropietarioID == noticia.NoticiaID).ToListAsync())
                {
                    db.Notas.Remove(n);
                }
                foreach (Archivo a in await db.Archivos.Where(a => a.PropietarioID == noticia.NoticiaID).ToListAsync())
                {
                    db.Archivos.Remove(a);
                }
                db.Noticias.Remove(noticia);
            }
            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporalesAsync
    }
}
