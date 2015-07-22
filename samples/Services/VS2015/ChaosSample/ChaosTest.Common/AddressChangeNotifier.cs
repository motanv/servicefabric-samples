//-----------------------------------------------------------------------
// <copyright file="AddressChangeNotifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using Microsoft.ServiceFabric.Services;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AddressChangeNotifier : IAddressChangeNotifier
    {
        private readonly Uri serviceName;
        private readonly ServiceNotificationFilterDescription filterDescription;
        private readonly FabricClient fabricClient;
        private long filterId;

        public AddressChangeNotifier(Uri serviceName)
            : this(serviceName, true)
        {
        }

        public AddressChangeNotifier(Uri serviceName, bool matchPrimaryOnly)
        {
            this.serviceName = serviceName;
            this.fabricClient = new FabricClient();
            this.filterDescription = new ServiceNotificationFilterDescription()
            {
                MatchPrimaryChangeOnly = matchPrimaryOnly,
                Name = this.serviceName
            };
        }

        public string Address
        {
            get;
            private set;
        }

        public async Task<string> GetAddressAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.Address))
            {
                ServicePartitionResolver spr = new ServicePartitionResolver(() => this.fabricClient);
                ResolvedServicePartition partition = await spr.ResolveAsync(this.serviceName, cancellationToken);

                this.Address = partition.GetEndpoint().Address;
            }

            return this.Address;
        }

        public async Task StartUpdatingAsync()
        {
            this.filterId = await this.fabricClient.ServiceManager.RegisterServiceNotificationFilterAsync(this.filterDescription);
            this.fabricClient.ServiceManager.ServiceNotificationFilterMatched += (s, e) => this.OnNotification(e);
        }

        public async Task StopUpdatingAsync()
        {
            await this.fabricClient.ServiceManager.UnregisterServiceNotificationFilterAsync(this.filterId);
        }

        private void OnNotification(EventArgs e)
        {
            ServiceNotification notification = ((FabricClient.ServiceManagementClient.ServiceNotificationEventArgs)e).Notification;

            ResolvedServiceEndpoint endpoint = notification.Endpoints.First(
                ep => ep.Role == ServiceEndpointRole.StatefulPrimary || ep.Role == ServiceEndpointRole.Stateless);

            this.Address = endpoint.Address;
        }
    }
}
