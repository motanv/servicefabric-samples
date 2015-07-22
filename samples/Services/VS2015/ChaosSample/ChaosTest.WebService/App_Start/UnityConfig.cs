//-----------------------------------------------------------------------
// <copyright file="UnityConfig.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{
    using ChaosTest.Common;
    using ChaosTest.WebService.Controllers;
    using Microsoft.Practices.Unity;
    using System;
    using System.Web.Http;
    using Unity.WebApi;

    public static class UnityConfig
    {
        public static void RegisterComponents(Uri chaosServiceName, HttpConfiguration config)
        {
            UnityContainer container = new UnityContainer();

            IAddressChangeNotifier addressNotifier = new AddressChangeNotifier(chaosServiceName);
            addressNotifier.StartUpdatingAsync().Wait();

            container.RegisterType<DefaultController>(new InjectionConstructor(addressNotifier));

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}
