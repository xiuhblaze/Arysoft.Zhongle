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
    public class PaginasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Paginas
        public async Task<ActionResult> Index(string buscar, string filtro, string orden, string status, int? pagina)
        {
            PaginaStatus myStatus = (PaginaStatus)(string.IsNullOrEmpty(status) ? PaginaStatus.Ninguno : Enum.Parse(typeof(PaginaStatus), status));

            ViewBag.MenuActive = "paginas";

            ViewBag.Orden = orden;
            ViewBag.OrdenTitulo = string.IsNullOrEmpty(orden) ? "titulo_desc" : "";
            ViewBag.OrdenFecha = orden == "fecha" ? "fecha_desc" : "fecha";
            ViewBag.OrdenVisitas = orden == "visitas" ? "visitas_desc" : "visitas";
            ViewBag.OrdenMeGusta = orden == "megusta" ? "megusta_desc" : "megusta";
            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;
            var paginas = db.Paginas
                .Include(p => p.PaginaPadre)
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Where(p => p.Status != PaginaStatus.Ninguno);
            if (!string.IsNullOrEmpty(buscar) || myStatus != PaginaStatus.Ninguno)
            {
                bool hayPalabras = !string.IsNullOrEmpty(buscar);

                paginas = paginas.Where(p =>
                    (
                        (!hayPalabras)
                        || p.Titulo.Contains(buscar)
                        || p.Resumen.Contains(buscar)
                        || p.HTMLContent.Contains(buscar)
                    )
                    && (myStatus == PaginaStatus.Ninguno ? true : p.Status == myStatus)
                );
            }
            switch (orden)
            {
                case "titulo_desc":
                    paginas = paginas.OrderByDescending(p => p.Titulo);
                    break;
                case "fecha":
                    paginas = paginas.OrderBy(p => p.FechaActualizacion);
                    break;
                case "fecha_desc":
                    paginas = paginas.OrderByDescending(p => p.FechaActualizacion);
                    break;
                case "visitas":
                    paginas = paginas.OrderBy(p => p.ContadorVisitas);
                    break;
                case "visitas_desc":
                    paginas = paginas.OrderBy(p => p.ContadorVisitas);
                    break;
                case "megusta":
                    paginas = paginas.OrderBy(p => p.MeGusta);
                    break;
                case "megusta_desc":
                    paginas = paginas.OrderByDescending(p => p.MeGusta);
                    break;
                default:
                    paginas = paginas.OrderBy(p => p.Titulo);
                    break;
            }
            List<PaginaIndexListViewModel> paginasViewModel = new List<PaginaIndexListViewModel>();
            foreach (Pagina miPagina in await paginas.ToListAsync())
            {
                paginasViewModel.Add(new PaginaIndexListViewModel(miPagina));
            }
            ViewBag.Count = paginasViewModel.Count();
            int numeroPagina = pagina ?? 1;
            int elementosPagina = Comun.ELEMENTOS_PAGINA;

            return View(paginasViewModel.ToPagedList(numeroPagina, elementosPagina));
        } // Index

        // GET: Admin/Paginas/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador";
                if (Request.IsAjaxRequest()) return Content("BadRequest");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagina pagina = await db.Paginas
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Include(p => p.PaginaPadre)
                .Include(p => p.PaginasHijo)
                .FirstOrDefaultAsync(p => p.PaginaID == id);
            if (pagina == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }

            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = true;
                TempData["isPage"] = true;
                PaginaDetailsViewModel p = new PaginaDetailsViewModel(pagina, "details", true);
                return PartialView("_details", p);
            }
            return View(pagina);
        } // Details

        // GET: Admin/Paginas/Create
        public async Task<ActionResult> Create()
        {
            await EliminarRegistrosTemporalesAsync(User.Identity.Name);

            Pagina pagina = new Pagina
            {
                PaginaID = Guid.NewGuid(),
                IndiceMenu = 0,
                Resumen = string.Empty,
                HTMLContent = string.Empty,
                Target = PaginaTarget.Ninguno,
                ContadorVisitas = 0,
                TieneGaleria = BoolTipo.Ninguno,
                FechaContador = DateTime.Now,
                Idioma = IdiomaTipo.Ninguno,
                EsPrincipal = BoolTipo.Ninguno,
                MeGusta = 0,
                Status = PaginaStatus.Ninguno,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now,
                UsuarioActualizacion = User.Identity.Name
            };

            db.Paginas.Add(pagina);

            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = pagina.PaginaID });
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "Ha ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
        } // Create

        // GET: Admin/Paginas/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                TempData["MessageBox"] = "No se recibió el identificador.";
                return RedirectToAction("Index");
            }
            Pagina pagina = await db.Paginas
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Include(p => p.PaginasHijo)
                .FirstOrDefaultAsync(p => p.PaginaID == id);
            if (pagina == null)
            {
                TempData["MessageBox"] = "No se encontró el registro.";
                return RedirectToAction("Index");
                //return HttpNotFound();
            }
            if (pagina.Status == PaginaStatus.Eliminada)
            {
                TempData["MessageBox"] = "La página ha sido eliminada.";
                return RedirectToAction("Index");
            }
            TempData["isReadonly"] = false;
            TempData["isPage"] = true;

            ViewBag.PaginaPadreID = await ListaPaginasPadreAsync(pagina.PaginaPadreID ?? Guid.Empty, pagina.PaginaID);

            return View(new PaginaEditViewModel(pagina));
        } // Edit

        // POST: Admin/Paginas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PaginaID,PaginaPadreID,Titulo,EtiquetaMenu,__resumen,__content,FriendlyUrl,TargetUrl,Target,TieneGaleria,ContadorVisitas,FechaContador,Idioma,EsPrincipal,__headScript,__footerScript,MeGusta,Status,FechaCreacion,OrdenPaginasHijo")] PaginaEditViewModel paginaVM)
        {
            Pagina pagina = paginaVM.ObtenerPagina();
            bool esNuevo = pagina.Status == PaginaStatus.Ninguno;

            if (esNuevo)
            {
                pagina.Status = PaginaStatus.Nueva;
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(paginaVM.OrdenPaginasHijo)) // Ordenar el indice de las páginas hijo
                {
                    string[] ids = paginaVM.OrdenPaginasHijo.Split(',');
                    int indice = 0;
                    foreach (var id in ids) // HACK: Esto se puede optimizar buscando diretamente la lista en la bdd
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            Pagina paginaHijo = await db.Paginas.FindAsync(new Guid(id));
                            if (paginaHijo != null)
                            {
                                paginaHijo.IndiceMenu = indice++;
                                db.Entry(paginaHijo).State = EntityState.Modified;
                            }
                        }
                    }
                }
                pagina.FechaActualizacion = DateTime.Now;
                pagina.UsuarioActualizacion = User.Identity.Name;
                db.Entry(pagina).State = EntityState.Modified;
                await db.SaveChangesAsync();
                if (esNuevo) { TempData["MessageBox"] = "Registro guardado satisfactoriamente."; }
                else { TempData["MessageBox"] = "Cambios guardados con exito."; }
                return RedirectToAction("Index");
            }

            TempData["isReadonly"] = false;
            TempData["isPage"] = true;
            ViewBag.PaginaPadreID = new SelectList(db.Paginas, "PaginaID", "Titulo", pagina.PaginaPadreID);
            return View(pagina);
        } // Edit

        // GET: Admin/Paginas/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                TempData["MessageBox"] = "No se recibió el identificador";
                if (Request.IsAjaxRequest()) return Content("BadRequest");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagina pagina = await db.Paginas
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Include(p => p.PaginaPadre)
                .Include(p => p.PaginasHijo)
                .FirstOrDefaultAsync(p => p.PaginaID == id);
            if (pagina == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }
            if (Request.IsAjaxRequest())
            {
                TempData["isReadonly"] = true;
                TempData["isPage"] = true;
                PaginaDetailsViewModel p = new PaginaDetailsViewModel(pagina, "delete", true);
                return PartialView("_details", p);
            }
            return View(pagina);
        } // Delete

        // POST: Admin/Paginas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            Pagina pagina = await db.Paginas.FindAsync(id);

            if (pagina.Status == PaginaStatus.Eliminada)
            {
                // Eliminando la carpeta de archivos de la página
                string dname = Path.Combine(Server.MapPath("~/Archivos/Paginas/"), pagina.PaginaID.ToString());
                if (Directory.Exists(dname)) { Directory.Delete(dname, true); }
                // Eliminando registros asociados a la página
                foreach (Nota n in await db.Notas.Where(n => n.PropietarioID == pagina.PaginaID).ToListAsync())
                {
                    db.Notas.Remove(n);
                }
                foreach (Archivo a in await db.Archivos.Where(a => a.PropietarioID == pagina.PaginaID).ToListAsync())
                {
                    db.Archivos.Remove(a);
                }
                // Eliminando la página
                db.Paginas.Remove(pagina);
            }
            else
            {
                pagina.Status = PaginaStatus.Eliminada;
                pagina.FechaActualizacion = DateTime.Now;
                pagina.UsuarioActualizacion = User.Identity.Name;
                db.Entry(pagina).State = EntityState.Modified;
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
            Pagina pagina = await db.Paginas.FindAsync(id);
            if (pagina == null)
            {
                TempData["MessageBox"] = "No se encontró el registro del identificador";
                if (Request.IsAjaxRequest()) return Content("HttpNotFound");
                return HttpNotFound();
            }
            pagina.Status = PaginaStatus.Nueva;
            pagina.FechaActualizacion = DateTime.Now;
            pagina.UsuarioActualizacion = User.Identity.Name;
            db.Entry(pagina).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                TempData["MessageBox"] = "A ocurrido una excepción: " + e.Message;
                return RedirectToAction("Index");
            }
            TempData["MessageBox"] = "La página ha sido reactivada.";

            return RedirectToAction("Index");
        } // Activar

        /// <summary>
        /// Obtiene una cadena de texto y la convierte en una url friendly, evitando existentes
        /// y evitanto compararla con la del ID que la mando generar.
        /// </summary>
        /// <param name="id">Identificador de la página que mando generar la url</param>
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
                count = await db.Paginas
                    .Where(p => p.FriendlyUrl == url
                        && p.PaginaID != id)
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



        // NOTAS

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

        // METODOS PRIVADOS

        /// <summary>
        /// Obtiene una lista de elementos para una lista desplegable, seleccionando la página padre
        /// y omitiendo la misma página que se va a presentar, para evitar redundancia circular.
        /// </summary>
        /// <param name="Seleccionado">Elemento previamente seleccinado, posiblemente la página padre</param>
        /// <param name="Omitir">Elemento a quitar de la lista, la página que se va a visualizar</param>
        /// <returns></returns>
        private async Task<List<SelectListItem>> ListaPaginasPadreAsync(Guid Seleccionado, Guid Omitir)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            lista.Add(new SelectListItem { Text = "(es página padre)", Value = Guid.Empty.ToString() });

            foreach (Pagina paginaPadre in await db.Paginas
                .Where(p => p.Status != PaginaStatus.Eliminada && p.Status != PaginaStatus.Ninguno)
                .ToListAsync())
            {
                if (paginaPadre.PaginaID != Omitir)
                {
                    lista.Add(new SelectListItem
                    {
                        Text = paginaPadre.Titulo,
                        Value = paginaPadre.PaginaID.ToString(),
                        Selected = paginaPadre.PaginaID == Seleccionado
                    });
                }
            }

            return lista;
        } // ListaPaginasPadreAsync

        private async Task EliminarRegistrosTemporalesAsync(string userName)
        {
            var paginas = await (from n in db.Paginas
                                 where n.Status == PaginaStatus.Ninguno
                                 && string.Compare(n.UsuarioActualizacion, userName, true) == 0
                                 select n).ToListAsync();

            foreach (var pagina in paginas)
            {
                string dname = Path.Combine(Server.MapPath("~/Archivos/Paginas/"), pagina.PaginaID.ToString());
                if (Directory.Exists(dname)) { Directory.Delete(dname, true); }

                foreach (Nota n in await db.Notas.Where(n => n.PropietarioID == pagina.PaginaID).ToListAsync())
                {
                    db.Notas.Remove(n);
                }
                foreach (Archivo a in await db.Archivos.Where(a => a.PropietarioID == pagina.PaginaID).ToListAsync())
                {
                    db.Archivos.Remove(a);
                }
                db.Paginas.Remove(pagina);
            }
            await db.SaveChangesAsync();
        } // EliminarRegistrosTemporalesAsync
    }
}
