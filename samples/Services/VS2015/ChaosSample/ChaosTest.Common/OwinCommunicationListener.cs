//-----------------------------------------------------------------------
// <copyright file="OwinCommunicationListener.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services;

    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly Func<ServiceInitializationParameters, string> serviceAddressDelegate;

        private IDisposable serverHandle;
        private readonly IOwinAppBuilder startup;
        private string publishAddress;
        private string listeningAddress;

        private CommonServiceEventSource eventSource;

        public OwinCommunicationListener(CommonServiceEventSource eventSource, Func<ServiceInitializationParameters, string> serviceAddressDelegate, IOwinAppBuilder startup)
        {
            this.serviceAddressDelegate = serviceAddressDelegate;
            this.startup = startup;
            this.eventSource = eventSource;
        }

        public bool ListenOnSecondary
        {
            get;
            set;
        }

        public void Initialize(ServiceInitializationParameters serviceInitializationParameters)
        {
            this.eventSource.Message("Initialize");
            this.listeningAddress = this.serviceAddressDelegate(serviceInitializationParameters);
            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            this.eventSource.Message("Opening on {0}", this.listeningAddress);

            try
            {
                // start the web server and the web socket listener
                this.StartWebServer(this.listeningAddress);

                return Task.FromResult(this.publishAddress);
            }
            catch (Exception e)
            {
                this.eventSource.Message("StartWebServer threw exception: {0}", e);

                Console.WriteLine(e);
                this.StopWebServer();
                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.eventSource.Message("CloseAsync");
            this.StopWebServer();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            this.eventSource.Message("Abort");
            this.StopWebServer();
        }

        /// <summary>
        /// This method starts Katana.
        /// </summary>
        /// 
        private void StartWebServer(string url)
        {
            this.serverHandle = WebApp.Start(url, appBuilder => this.startup.Configuration(appBuilder));
        }

        private void StopWebServer()
        {
            if (this.serverHandle != null)
            {
                try
                {
                    this.serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
