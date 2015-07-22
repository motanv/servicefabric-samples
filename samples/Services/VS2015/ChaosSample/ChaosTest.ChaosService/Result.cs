//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Collections.Generic;

    public class Result
    {
        public string TotalRuntime;
        public string CurrentState;
        public SortedList<long, ChaosEntry> ChaosLog;
    }
}
