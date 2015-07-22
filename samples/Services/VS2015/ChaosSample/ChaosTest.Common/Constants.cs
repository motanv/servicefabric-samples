//-----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.Common
{
    public static class Constants
    {
        #region message strings

        public static readonly string IncorrectTypeMessage = @"serviceInitializationParameters is not of type StatefulServiceInitializationParameters";

        #endregion

        #region labels

        public static readonly string ChaosServiceEndpointName = @"ChaosServiceEndpoint";
        public static readonly string ChaosServiceStateDictionaryName = @"CurrentState";
        public static readonly string ChaosDictionaryLabel = @"ChaosDictionary";
        public static readonly string ChaosDictionaryKeyLabel = @"ChaosDictionaryKey";
        public static readonly string StartTimeLabel = @"StartTime";

        #endregion

        #region miscellaneous

        public static readonly int HistoryLength = 30;
        public static readonly int ServiceRequestRetryCount = 3;

        #endregion
    }
}
