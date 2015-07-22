//-----------------------------------------------------------------------
// <copyright file="ServiceEventSource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System.Diagnostics.Tracing;
    using Common;

    [EventSource(Name = "MyCompany-ServiceApplication-ChaosTestAppChaosWebService")]
    internal sealed class ServiceEventSource : CommonServiceEventSource
    {
        public static ServiceEventSource Current = new ServiceEventSource();
    }
}
