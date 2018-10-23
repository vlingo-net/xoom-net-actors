// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class BasicSupervisionTest : ActorsTest
    {
        [Fact]
        public void TestPublicRootDefaultParentSupervisor()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), World.DefaultParent, "failure-for-default"));
            failureControlTestResults.UntilFailNow = Until(1);
            Assert.Equal(0, failureControlTestResults.FailNowCount.Get());

            failure.FailNow();
            failureControlTestResults.UntilFailNow.Completes();
            Assert.Equal(1, failureControlTestResults.FailNowCount.Get());

            failureControlTestResults.UntilAfterFail = Until(1);
            Assert.Equal(0, failureControlTestResults.AfterFailureCount.Get());
            failure.AfterFailure();
            failureControlTestResults.UntilAfterFail.Completes();
            Assert.Equal(1, failureControlTestResults.AfterFailureCount.Get());
        }

        [Fact]
        public void TestStoppingSupervisor()
        {
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<StoppingSupervisorActor>(Definition.NoParameters, "stopping-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure-for-stop"));

            Assert.Equal(0, failureControlTestResults.FailNowCount.Get());
            failure.Actor.FailNow();
            Assert.Equal(1, failureControlTestResults.FailNowCount.Get());

            Assert.Equal(0, failureControlTestResults.AfterFailureCount.Get());
            failure.Actor.AfterFailure();
            Assert.Equal(0, failureControlTestResults.AfterFailureCount.Get());

            Assert.Equal(1, failureControlTestResults.AfterStopCount.Get());
        }

        [Fact]
        public void TestRestartSupervisor()
        {
            var restartSupervisorTestResults = new RestartSupervisorTestResults();
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<RestartSupervisorActor>(
                    Definition.Parameters(restartSupervisorTestResults), "restart-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure-for-restart"));

            Assert.Equal(0, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(0, restartSupervisorTestResults.InformedCount.Get());
            Assert.Equal(0, failureControlTestResults.AfterRestartCount.Get());
            Assert.Equal(0, failureControlTestResults.AfterStopCount.Get());
            Assert.Equal(0, failureControlTestResults.BeforeRestartCount.Get());
            Assert.Equal(1, failureControlTestResults.BeforeStartCount.Get());

            failure.Actor.FailNow();

            Assert.Equal(1, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(1, restartSupervisorTestResults.InformedCount.Get());
            Assert.Equal(1, failureControlTestResults.AfterRestartCount.Get());
            Assert.Equal(1, failureControlTestResults.AfterStopCount.Get());
            Assert.Equal(1, failureControlTestResults.BeforeRestartCount.Get());
            Assert.Equal(2, failureControlTestResults.BeforeStartCount.Get());

            Assert.Equal(0, failureControlTestResults.AfterFailureCount.Get());
            failure.Actor.AfterFailure();
            Assert.Equal(1, failureControlTestResults.AfterFailureCount.Get());

            Assert.Equal(0, failureControlTestResults.StoppedCount.Get());
        }


        private class RestartSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; } = new AtomicInteger(0);
        }

        private class StoppingSupervisorActor : Actor, ISupervisor
        {
            public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

            public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

            public void Inform(Exception error, ISupervised supervised)
            {
                Logger.Log($"StoppingSupervisorActor informed of failure in: {supervised.Address.Name} because: {error.Message}", error);
                supervised.Stop(SupervisionStrategy.Scope);
            }

            private class SupervisionStrategyImpl : ISupervisionStrategy
            {
                public int Intensity => SupervisionStrategyConstants.DefaultIntensity;

                public long Period => SupervisionStrategyConstants.DefaultPeriod;

                public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
            }
        }

        private class RestartSupervisorActor : Actor, ISupervisor
        {
            private readonly RestartSupervisorTestResults testResults;

            public RestartSupervisorActor(RestartSupervisorTestResults testResults)
            {
                this.testResults = testResults;
            }

            public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

            public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

            public void Inform(Exception error, ISupervised supervised)
            {
                Logger.Log($"RestartSupervisorActor informed of failure in: {supervised.Address.Name} because: {error.Message}", error);
                supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
                testResults.InformedCount.IncrementAndGet();
            }

            private class SupervisionStrategyImpl : ISupervisionStrategy
            {
                public int Intensity => 2;

                public long Period => SupervisionStrategyConstants.DefaultPeriod;

                public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
            }
        }
    }
}
