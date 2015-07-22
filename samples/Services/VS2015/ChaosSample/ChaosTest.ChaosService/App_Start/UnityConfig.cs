//-----------------------------------------------------------------------
// <copyright file="UnityConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System;
    using System.Web.Http;
    using ChaosTest.ChaosService.Controllers;
    using Microsoft.Practices.Unity;
    using Unity.WebApi;

    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config, Service service)
        {
            try
            {
                UnityContainer container = new UnityContainer();
                container.RegisterType<DefaultController>(new InjectionConstructor(service));
                config.DependencyResolver = new UnityDependencyResolver(container);
            }
            catch (Exception e)
            {
                Console.WriteLine("UU -> {0}", e);
            }
        }
    }
}
