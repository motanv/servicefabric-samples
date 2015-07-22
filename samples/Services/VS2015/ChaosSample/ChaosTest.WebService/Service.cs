//-----------------------------------------------------------------------
// <copyright file="Service.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{

    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using ChaosTest.Common;
    using Microsoft.ServiceFabric.Services;

    public class Service : StatelessService
    {
        protected override ICommunicationListener CreateCommunicationListener()
        {
            string applicationName = FabricRuntime.GetActivationContext().ApplicationName;

            Uri chaosServiceName 
                = new Uri(
                    string.Format(
                        CultureInfo.InvariantCulture, "{0}/{1}", 
                        applicationName, 
                        ChaosTestWebServiceConfigSettings.ChaosServiceName));

            ServiceEventSource.Current.ServiceMessage(this, "App Name: {0}, ChaosServiceName: {1}, Actual Name: {2}", 
                applicationName, 
                ChaosTestWebServiceConfigSettings.ChaosServiceName, 
                chaosServiceName);

            return new OwinCommunicationListener(
                ServiceEventSource.Current, this.GetServiceAddress, new Startup(chaosServiceName));
        }

        public string GetServiceAddress(ServiceInitializationParameters serviceInitializationParameters)
        {
            EndpointResourceDescription serviceEndpoint 
                = serviceInitializationParameters.CodePackageActivationContext.GetEndpoint("ChaosWebServiceEndpoint");

            string address = string.Format(
                CultureInfo.InvariantCulture,
                "http://+:{0}/ChaosTest",
                serviceEndpoint.Port);

            return address;
        }
    }
}
