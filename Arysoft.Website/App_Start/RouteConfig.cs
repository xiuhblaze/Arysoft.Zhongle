using System.Web.Mvc;
using System.Web.Routing;

namespace Arysoft.Website
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }                
            );

            routes.MapRoute(
                name: "Noticias",
                url: "Noticias/{friendlyUrl}",
                defaults: new { controller = "Home", action = "Noticia", friendlyUrl = UrlParameter.Optional });

            routes.MapRoute(
                name: "FriendlyUrl",
                url: "{friendlyUrl}",
                defaults: new { controller = "Home", action = "Pagina", friendlyUrl = UrlParameter.Optional });
        }
    }
}