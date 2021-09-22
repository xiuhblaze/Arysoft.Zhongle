using System;
using Arysoft.Website.Models;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Arysoft.Website
{
    // Note: For instructions on enabling IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=301868
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //ControllerBuilder.Current.DefaultNamespaces.Add("Arysoft.Website.Controllers");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            System.Globalization.CultureInfo unCI = System.Globalization.CultureInfo.CreateSpecificCulture("es-MX");

            System.Threading.Thread.CurrentThread.CurrentCulture = unCI;
            System.Threading.Thread.CurrentThread.CurrentUICulture = unCI;
        }
    }
}
