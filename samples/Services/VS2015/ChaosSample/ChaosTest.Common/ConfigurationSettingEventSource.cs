//-----------------------------------------------------------------------
// <copyright file="ConfigurationSettingEventSource.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.Common
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "MyCompany-ServiceApplication-ChaosTestAppChaosCommonConfigurationSetting")]
    internal sealed class ConfigurationSettingEventSource : CommonServiceEventSource
    {
        public static ConfigurationSettingEventSource Current = new ConfigurationSettingEventSource();
    }
}
