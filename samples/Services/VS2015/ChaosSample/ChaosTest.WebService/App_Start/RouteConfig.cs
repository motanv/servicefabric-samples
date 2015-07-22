//-----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System.Web.Http;

    public static class RouteConfig
    {
        /// <summary>
        /// Routing registration.
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "{action}",
                defaults: new { controller = "Default", action = "Index" },
                constraints: new { }
            );

            routes.MapHttpRoute(
                name: "Scripts",
                routeTemplate: "Scripts/{name}",
                defaults: new { controller = "Scripts", action = "Get" },
                constraints: new { }
            );

            routes.MapHttpRoute(
                name: "Images",
                routeTemplate: "Images/{name}",
                defaults: new { controller = "Images", action = "Get" },
                constraints: new { }
            );
        }
    }
}
