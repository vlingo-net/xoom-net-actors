// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
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

            var access = failureControlTestResults.AfterCompleting(3);

            failure.FailNow();

            Assert.Equal(1, access.ReadFrom<int>("failNowCount"));
            Assert.Equal(1, access.ReadFromExpecting("afterRestartCount", 1));

            access = failureControlTestResults.AfterCompleting(1);

            failure.AfterFailure();

            Assert.Equal(1, access.ReadFromExpecting<int>("afterFailureCount", 1));
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

            var access = failureControlTestResults.AfterCompleting(2);

            failure.Actor.FailNow();

            Assert.Equal(1, access.ReadFrom<int>("failNowCount"));

            failure.Actor.AfterFailure();

            Assert.Equal(1, access.ReadFrom<int>("stoppedCount"));

            Assert.Equal(0, access.ReadFrom<int>("afterFailureCount"));
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

            var failureAccess = failureControlTestResults.AfterCompleting(6);
            var restartAccess = restartSupervisorTestResults.AfterCompleting(1);

            failure.Actor.FailNow();

            Assert.Equal(1, restartAccess.ReadFrom<int>("informedCount"));
            Assert.Equal(2, failureAccess.ReadFrom<int>("beforeStartCount"));
            Assert.Equal(1, failureAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(1, failureAccess.ReadFrom<int>("afterRestartCount"));
            Assert.Equal(1, failureAccess.ReadFrom<int>("afterStopCount"));
            Assert.Equal(1, failureAccess.ReadFrom<int>("beforeRestartCount"));

            var afterFailureAccess = failureControlTestResults.AfterCompleting(1);

            failure.Actor.AfterFailure();

            Assert.Equal(1, afterFailureAccess.ReadFrom<int>("afterFailureCount"));
            Assert.Equal(0, afterFailureAccess.ReadFrom<int>("stoppedCount"));
        }


        private class RestartSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; } = new AtomicInteger(0);
            public AccessSafely Access { get; private set; }

            public RestartSupervisorTestResults()
            {
                Access = AfterCompleting(0);
            }

            public AccessSafely AfterCompleting(int times)
            {
                Access = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("informedCount", (int increment) => InformedCount.Set(InformedCount.Get() + increment))
                    .ReadingWith("informedCount", () => InformedCount.Get());

                return Access;
            }
        }

        private class StoppingSupervisorActor : Actor, ISupervisor
        {
            public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

            public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

            public void Inform(Exception error, ISupervised supervised)
            {
                // Logger.Log($"StoppingSupervisorActor informed of failure in: {supervised.Address.Name} because: {error.Message}", error);
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
                // Logger.Log($"RestartSupervisorActor informed of failure in: {supervised.Address.Name} because: {error.Message}", error);
                supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
                testResults.Access.WriteUsing("informedCount", 1);
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
