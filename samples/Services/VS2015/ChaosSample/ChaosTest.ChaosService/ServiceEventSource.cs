//-----------------------------------------------------------------------
// <copyright file="ChaosServiceEventSource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Diagnostics.Tracing;
    using Common;

    [EventSource(Name = "MyCompany-ServiceApplication-ChaosTestAppChaosService")]
    internal sealed class ServiceEventSource : CommonServiceEventSource
    {
        public static ServiceEventSource Current = new ServiceEventSource();
    }
}
