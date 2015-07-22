//-----------------------------------------------------------------------
// <copyright file="ChaosTestWebServiceConfigSettings.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.WebService
{
    using ChaosTest.Common;
    using System.Fabric;
    using System.Fabric.Description;

    class ChaosTestWebServiceConfigSettings
    {
        // WinFab-based config
        private const string ConfigPackageName = "Config";
        private const string ChaosTestConfigurationSectionName = "ChaosTestWebService";
        private static readonly ConfigurationSettings ConfigurationSettings =
                   FabricRuntime.GetActivationContext().GetConfigurationPackageObject(ConfigPackageName).Settings;

        public static string ChaosServiceName = new ConfigurationSetting<string>(
                ConfigurationSettings, 
                ChaosTestConfigurationSectionName, 
                "ChaosServiceName", 
                "ChaosTestService",
                ServiceEventSource.Current).Value;
    }
}
