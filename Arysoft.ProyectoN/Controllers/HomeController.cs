using System;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using Arysoft.ProyectoN.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace Arysoft.ProyectoN.Controllers
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

    }
}
