using System.Web.Http;

namespace AStar
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("Path", "Path/{startStreetName}/{stopStreetName}", new { controller = "Values", action = "FindPath" });
            config.Routes.MapHttpRoute("NearestNodes", "NearestNodes/{lat}/{lng}/{radius}/{limit}", new { controller = "Values", action = "NearestNodes" });

            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }
    }
}