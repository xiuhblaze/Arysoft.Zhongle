using System;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using Arysoft.Website.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using PagedList;

namespace Arysoft.Website.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {

            //var SeccionesList = new SelectList(db.Secciones.Where(s => s.Status == StatusTipo.Activo).OrderBy(s => s.Numero), "SeccionID", "Numero").ToList();

            //SeccionesList.Insert(0, new SelectListItem { Text = "(seleccionar)", Value = Guid.Empty.ToString() });
            //ViewBag.SeccionID = SeccionesList;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // NOTICIAS

        [Route("Noticias/")]
        public async Task<ActionResult> Noticias(string buscar, string filtro, int? pagina)
        {

            if (buscar != null)
            {
                pagina = 1;
                buscar = buscar.Trim();
            }
            else { buscar = filtro ?? string.Empty; }
            ViewBag.Filtro = buscar;
            var noticias = db.Noticias
                .Where(n => n.Status == NoticiaStatus.Publicada);
            if (!string.IsNullOrEmpty(buscar))
            {
                noticias = noticias.Where(n =>
                    n.Titulo.Contains(buscar)
                    || n.Resumen.Contains(buscar)
                    || n.HTMLContent.Contains(buscar)
                );
            }
            noticias = noticias.OrderByDescending(n => n.FechaPublicacion);

            List<NoticiasListViewModel> noticiasViewModel = new List<NoticiasListViewModel>();
            foreach (Noticia noticia in await noticias.ToListAsync())
            {
                noticiasViewModel.Add(new NoticiasListViewModel(noticia));
            }
            ViewBag.Count = noticiasViewModel.Count();
            int numeroPagina = pagina ?? 1;
            int elementosPagina = 25;

            return View(noticiasViewModel.ToPagedList(numeroPagina, elementosPagina));
        } // Noticias

        [Route("Noticias/{friendlyUrl}")]
        public async Task<ActionResult> Noticia(string friendlyUrl)
        {
            Noticia noticia = await db.Noticias
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Where(n => n.FriendlyUrl == friendlyUrl)
                .FirstOrDefaultAsync();

            if (noticia == null)
            {
                TempData["MessageBox"] = "La noticia no se encuentra.";
                return RedirectToAction("Index");
            }

            if (noticia.Status != NoticiaStatus.Publicada)
            {
                TempData["MessageBox"] = "La noticia no existe.";
                return RedirectToAction("Index");
            }

            noticia.ContadorVisitas += 1;
            db.Entry(noticia).State = EntityState.Modified;
            await db.SaveChangesAsync();

            return View(noticia);
        } // Noticia

        /// <summary>
        /// Obtiene las últimas noticias de acuerdo al tipo solicitado
        /// 1: Recientes, 2: Vistas, 3:MeGusta
        /// </summary>
        /// <param name="cantidad">Cantidad de noticias a obtener</param>
        /// <param name="tipo">Tipo de noticias por las cual filtrar y ordenar</param>
        /// <returns></returns>
        public async Task<ActionResult> NoticiasTop(int cantidad, int tipo, bool conRetorno)
        {
            var noticias = db.Noticias
                .Where(n => n.Status == NoticiaStatus.Publicada);
            switch (tipo)
            {
                case 1:
                    noticias = noticias.OrderByDescending(n => n.FechaPublicacion);
                    break;
                case 2:
                    noticias = noticias.OrderByDescending(n => n.ContadorVisitas);
                    break;
                case 3:
                    noticias = noticias.OrderByDescending(n => n.MeGusta);
                    break;
            }
            noticias = noticias.Take(cantidad);

            List<NoticiasListViewModel> noticiasViewModel = new List<NoticiasListViewModel>();
            foreach (Noticia noticia in await noticias.ToListAsync())
            {
                noticiasViewModel.Add(new NoticiasListViewModel(noticia));
            }

            return PartialView("_noticiasTop", noticiasViewModel);
        } // NoticiasTop

        // PAGINAS

        [Route("Paginas/{friendlyUrl}")]
        public async Task<ActionResult> Pagina(string friendlyUrl)
        {
            Pagina pagina = await db.Paginas
                .Include(p => p.Archivos)
                .Include(p => p.Notas)
                .Include(p => p.PaginaPadre)
                .Include(p => p.PaginasHijo)
                .Where(p => p.FriendlyUrl == friendlyUrl)
                .FirstOrDefaultAsync();

            if (pagina == null)
            {
                TempData["MessageBox"] = "La página no se encuentra.";
                return RedirectToAction("Index");
            }

            if (pagina.Status != PaginaStatus.Publicada)
            {
                TempData["MessageBox"] = "La página no existe.";
                return RedirectToAction("Index");
            }

            pagina.ContadorVisitas += 1;
            db.Entry(pagina).State = EntityState.Modified;
            await db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(pagina.TargetUrl))
            {
                return Redirect(pagina.TargetUrl);
            }

            pagina.Archivos = pagina.Archivos.OrderBy(a => a.Nombre).ToList();
            pagina.Notas = pagina.Notas.OrderByDescending(n => n.FechaCreacion).ToList();
            pagina.PaginasHijo = pagina.PaginasHijo.OrderBy(p => p.IndiceMenu).ToList();

            return View(pagina);
        }
    }
}
