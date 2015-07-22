//-----------------------------------------------------------------------
// <copyright file="Service.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Fabric.Testability;
    using System.Fabric.Testability.Scenario;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services;

    public class Service : StatefulService
    {
        public Service()
        {
            this.StopEventTokenSource = new CancellationTokenSource();
        }

        public CancellationTokenSource StopEventTokenSource { get; set; }

        public string GetServiceAddress(ServiceInitializationParameters serviceInitializationParameters)
        {
            StatefulServiceInitializationParameters statefulInitParams = serviceInitializationParameters as StatefulServiceInitializationParameters;

            Trace.Assert(statefulInitParams != null, Constants.IncorrectTypeMessage);

            EndpointResourceDescription serviceEndpoint 
                = serviceInitializationParameters.CodePackageActivationContext.GetEndpoint(Constants.ChaosServiceEndpointName);

            string address = string.Format(
                CultureInfo.InvariantCulture,
                "http://+:{0}/{1}/{2}/{3}",
                serviceEndpoint.Port,
                statefulInitParams.PartitionId,
                statefulInitParams.ReplicaId,
                Guid.NewGuid());

            return address;
        }

        protected override ICommunicationListener CreateCommunicationListener()
        {
            return new OwinCommunicationListener(ServiceEventSource.Current, this.GetServiceAddress, new Startup(this));
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            bool validationExeptionCaught = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    IReliableDictionary<string, CurrentState> currentStateDict =
                        await this.StateManager.GetOrAddAsync<IReliableDictionary<string, CurrentState>>(Constants.ChaosServiceStateDictionaryName);

                    using (ITransaction tx = this.StateManager.CreateTransaction())
                    {
                        if (!await currentStateDict.ContainsKeyAsync(tx, Constants.ChaosServiceStateDictionaryName))
                        {
                            await currentStateDict.AddAsync(tx, Constants.ChaosServiceStateDictionaryName, CurrentState.Stopped);
                        }

                        bool toContinue = false;
                        ConditionalResult<CurrentState> currentStateResult = await currentStateDict.TryGetValueAsync(tx, Constants.ChaosServiceStateDictionaryName);
                        if (currentStateResult.HasValue)
                        {
                            if (currentStateResult.Value == CurrentState.Stopped
                                || currentStateResult.Value == CurrentState.None)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                                toContinue = true;
                            }
                        }

                        await tx.CommitAsync();

                        if (toContinue)
                        {
                            continue;
                        }
                    }

                    // Create new cancellation source since old one might have been cancelled. 
                    this.StopEventTokenSource = new CancellationTokenSource();
                    CancellationToken stopEventToken = this.StopEventTokenSource.Token;

                    using (
                        CancellationTokenSource linkedCts =
                            CancellationTokenSource.CreateLinkedTokenSource(stopEventToken, cancellationToken))
                    {
                        if (validationExeptionCaught)
                        {
                            await Task.Delay(ChaosTestConfigSettings.MaxClusterStabilizationTimeout, linkedCts.Token);
                            validationExeptionCaught = false;
                        }

                        FabricClient fabricClient = new FabricClient();
                        ChaosTestScenarioParameters chaosScenarioParameters =
                            new ChaosTestScenarioParameters(
                                ChaosTestConfigSettings.MaxClusterStabilizationTimeout,
                                ChaosTestConfigSettings.MaxConcurrentFaults,
                                ChaosTestConfigSettings.EnableMoveReplicaFaults,
                                TimeSpan.MaxValue)
                            {
                                WaitTimeBetweenFaults =
                                        ChaosTestConfigSettings.WaitTimeBetweenFaults,
                                WaitTimeBetweenIterations =
                                        ChaosTestConfigSettings.WaitTimeBetweenIterations
                            };

                        ChaosTestScenario chaosTestScenario = new ChaosTestScenario(fabricClient, chaosScenarioParameters);

                        chaosTestScenario.ProgressChanged += this.TestScenarioProgressChanged;

                        await chaosTestScenario.ExecuteAsync(linkedCts.Token);
                    }
                }
                catch (TimeoutException e)
                {
                    string formatString = $"Caught TimeoutException '{e.Message}'. Will wait for cluster to stabilize before continuing test";
                    ServiceEventSource.Current.ServiceMessage(this, "{0}", formatString);
                    validationExeptionCaught = true;
                    await this.StoreEventAsync(formatString);
                }
                catch (FabricValidationException e)
                {
                    string formatString = $"Caught FabricValidationException '{e.Message}'. Will wait for cluster to stabilize before continuing test";
                    ServiceEventSource.Current.ServiceMessage(this, "{0}", formatString);
                    validationExeptionCaught = true;
                    await this.StoreEventAsync(formatString);
                }
                catch (OperationCanceledException)
                {
                    ServiceEventSource.Current.ServiceMessage(this, "{0}", "Caught OperationCanceledException Exception during test excecution. This is expected if test was stopped");
                }
                catch (AggregateException e)
                {
                    if (e.InnerException is OperationCanceledException)
                    {
                        ServiceEventSource.Current.ServiceMessage(this, "{0}", "Caught OperationCanceledException Exception during test excecution. This is expected if test was stopped");
                    }
                    else
                    {
                        string formatString = $"Caught unexpected Exception during test excecution {e.InnerException}";
                        ServiceEventSource.Current.ServiceMessage(this, "{0}", formatString);
                        await this.StoreEventAsync(formatString);
                    }
                }
                catch (Exception e)
                {
                    string formatString = $"Caught unexpected Exception during test excecution {e}";
                    ServiceEventSource.Current.ServiceMessage(this, "{0}", formatString);
                    await this.StoreEventAsync(formatString);
                }
            }
        }

        private void TestScenarioProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string eventString = e.UserState.ToString();

            // Chaos Test Scenario runs in units of iterations; but from this sample's 
            // Chaos service point of view, iteration does not make much sense; hence, 
            // getting rid of pre- and post-, ambles for iterations.
            if (eventString.StartsWith("Running iteration"))
            {
                return;
            }

            if (eventString.StartsWith("Scenario complete"))
            {
                return;
            }

            this.StoreEventAsync(eventString).Wait();
        }

        private async Task StoreEventAsync(string eventString)
        {
            ServiceEventSource.Current.ServiceMessage(this, "ChaosTest: {0}", eventString);

            IReliableDictionary<string, long> key = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>(Constants.ChaosDictionaryKeyLabel);
            IReliableDictionary<string, DateTime> startTime = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, DateTime>>(Constants.StartTimeLabel);
            IReliableDictionary<long, ChaosEntry> chaosDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<long, ChaosEntry>>(Constants.ChaosDictionaryLabel);

            using (ITransaction tx = this.StateManager.CreateTransaction())
            {
                if (! await startTime.ContainsKeyAsync(tx, Constants.StartTimeLabel))
                {
                    await startTime.AddAsync(tx, Constants.StartTimeLabel, DateTime.UtcNow);
                }

                if (! await key.ContainsKeyAsync(tx, Constants.ChaosDictionaryKeyLabel))
                {
                    await key.AddAsync(tx, Constants.ChaosDictionaryKeyLabel, 0);
                }

                ConditionalResult<long> result = await key.TryGetValueAsync(tx, Constants.ChaosDictionaryKeyLabel);

                if (result.HasValue)
                {
                    long currentKey = result.Value;
                    if (chaosDictionary.Count() > Constants.HistoryLength)
                    {
                        await chaosDictionary.TryRemoveAsync(tx, currentKey - Constants.HistoryLength);
                    }

                    ChaosEntry chaosEntry = new ChaosEntry
                    {
                        Record = eventString,
                        TimeStamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                    };

                    await chaosDictionary.AddAsync(tx, ++currentKey, chaosEntry);
                    await key.SetAsync(tx, Constants.ChaosDictionaryKeyLabel, currentKey);

                    await tx.CommitAsync();
                }
            }
        }
    }
}
