//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Web.Http;
    using Common;
    using Owin;

    /// <summary>
    /// OWIN configuration
    /// </summary>
    public class Startup : IOwinAppBuilder
    {
        private readonly Service service;

        public Startup(Service service)
        {
            this.service = service;
        }

        /// <summary>
        /// Hooks up the Web API pipeline with the OWIN pipeline,
        /// using the method UseWebApi
        /// </summary>
        /// <param name="appBuilder"></param>
        /// 
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            FormatterConfig.ConfigureFormatters(config.Formatters);
            RouteConfig.RegisterRoutes(config.Routes);
            UnityConfig.RegisterComponents(config, this.service);

            appBuilder.UseWebApi(config);
        }
    }
}
