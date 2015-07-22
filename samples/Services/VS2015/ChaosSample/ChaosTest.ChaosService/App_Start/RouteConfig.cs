//-----------------------------------------------------------------------
// <copyright file="RouteConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Web.Http;

    public static class RouteConfig
    {
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "{action}",
                defaults: new { controller = "Default", action = "Index" },
                constraints: new { });
        }
    }
}
