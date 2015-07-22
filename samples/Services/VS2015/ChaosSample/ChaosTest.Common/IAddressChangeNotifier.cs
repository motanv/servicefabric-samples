//-----------------------------------------------------------------------
// <copyright file="IAddressChangeNotifier.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.Common
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAddressChangeNotifier
    {
        Task<string> GetAddressAsync(CancellationToken cancellationToken);

        Task StartUpdatingAsync();

        Task StopUpdatingAsync();
    }
}
