//-----------------------------------------------------------------------
// <copyright file="DefaultController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using Microsoft.ServiceFabric.Data.Collections;

    /// <summary>
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        private readonly Service service;

        public DefaultController(Service service)
        {
            this.service = service;
        }

        [HttpPost]
        public IHttpActionResult Start()
        {
            try
            {
                var currentStateDict = this.service.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>("CurrentState").Result;
                using (var tx = this.service.StateManager.CreateTransaction())
                {
                    var currentStateResult = currentStateDict.TryGetValueAsync(tx, "CurrentState", LockMode.Default).Result;
                    if (currentStateResult.HasValue)
                    {
                        if (currentStateResult.Value == CurrentState.Stopped
                            || currentStateResult.Value == CurrentState.None)
                        {
                            currentStateDict.AddOrUpdateAsync(tx, "CurrentState", CurrentState.Running, (key, existingValue) => CurrentState.Running).Wait();
                        }
                    }

                    tx.CommitAsync().Wait();
                }

                return this.Ok();
            }
            catch
            {
                return this.InternalServerError();
            }
        }

        [HttpPost]
        public IHttpActionResult Stop()
        {
            try
            {
                var currentStateDict = this.service.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>("CurrentState").Result;
                bool shouldCancel = false;
                using (var tx = this.service.StateManager.CreateTransaction())
                {
                    var currentStateResult = currentStateDict.TryGetValueAsync(tx, "CurrentState", LockMode.Update).Result;
                    if (currentStateResult.HasValue)
                    {
                        if (currentStateResult.Value == CurrentState.Running)
                        {
                            currentStateDict.AddOrUpdateAsync(tx, "CurrentState", CurrentState.Stopped, (key, existingValue) => CurrentState.Stopped).Wait();
                            shouldCancel = true;
                        }
                    }

                    var seq = tx.CommitAsync().Result;
                }

                if (shouldCancel)
                {
                    this.service.StopEventTokenSource.Cancel();
                }

                return this.Ok();
            }
            catch
            {
                return this.InternalServerError();
            }
        }

        [HttpGet]
        public Result Results()
        {
            var results = new Result { ChaosLog = new SortedList<long, ChaosEntry>(), CurrentState = "NA", TotalRuntime = "NA" };
            var chaosDictionary = this.service.StateManager.GetOrAddAsync<IReliableDictionary<long, ChaosEntry>>("ChaosDictionary").Result;
            var startTime = this.service.StateManager.GetOrAddAsync<IReliableDictionary<string, DateTime>>("StartTime").Result;
            var currentState = this.service.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>("CurrentState").Result;
            using (var tx = this.service.StateManager.CreateTransaction())
            {
                foreach (var kvp in chaosDictionary)
                {
                    results.ChaosLog.Add(kvp.Key, kvp.Value);
                }

                var result = startTime.TryGetValueAsync(tx, "StartTime").Result;
                if (result.HasValue)
                {
                    results.TotalRuntime = (DateTime.UtcNow - result.Value).ToString();
                }

                var currentStateResult = currentState.TryGetValueAsync(tx, "CurrentState").Result;
                if (currentStateResult.HasValue)
                {
                    results.CurrentState = currentStateResult.Value.ToString();
                }

                var seq = tx.CommitAsync().Result;
            }

            return results;
        }
    }
}
