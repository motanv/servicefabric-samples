//-----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{
    using ChaosTest.Common;
    using Owin;
    using System;
    using System.Web.Http;

    /// <summary>
    /// OWIN configuration
    /// </summary>
    public class Startup : IOwinAppBuilder
    {
        private Uri chaosServiceName;

        public Startup(Uri chaosServiceName)
        {
            this.chaosServiceName = chaosServiceName;
        }

        /// <summary>
        /// Configures the app builder using Web API.
        /// </summary>
        /// <param name="appBuilder"></param>
        /// 
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            FormatterConfig.ConfigureFormatters(config.Formatters);
            RouteConfig.RegisterRoutes(config.Routes);
            UnityConfig.RegisterComponents(this.chaosServiceName, config);

            appBuilder.UseWebApi(config);
        }
    }
}
