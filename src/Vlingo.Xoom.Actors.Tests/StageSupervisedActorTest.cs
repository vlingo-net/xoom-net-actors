// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.Tests.Supervision;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class StageSupervisedActorTest : ActorsTest
    {
        [Fact]
        public void TestExpectedAttributes()
        {
            var testResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(() => new FailureControlActor(testResults), "failure");
            var exception = new ApplicationException("Failed");

            var supervised = new StageSupervisedActor<IFailureControl>(failure.ActorInside, exception);

            Assert.Equal("failure", supervised.Address.Name);
            Assert.Equal(World.DefaultSupervisor, supervised.Supervisor);
            Assert.Equal(exception, supervised.Error);
        }

        [Fact]
        public void TestEscalate()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), "failure"));
            var exception = new ApplicationException("Failed");
            var supervised = new StageSupervisedActor<IFailureControl>(failure.ActorInside, exception);

            var access = failureControlTestResults.AfterCompleting(1);

            supervised.Escalate();

            Assert.Equal(1, access.ReadFrom<int>("stoppedCount"));
        }

        [Fact]
        public void TestRestart()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), "failure"));
            var exception = new ApplicationException("Failed");
            var supervised = new StageSupervisedActor<IFailureControl>(failure.ActorInside, exception);

            var access = failureControlTestResults.AfterCompleting(4);

            supervised.RestartWithin(1000, 5, SupervisionStrategyConstants.Scope.One);

            Assert.Equal(2, access.ReadFrom<int>("beforeStartCount"));
            Assert.Equal(1, access.ReadFrom<int>("beforeRestartCount"));
            Assert.Equal(1, access.ReadFrom<int>("afterRestartCount"));
        }

        [Fact]
        public void TestSuspendResume()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), "failure"));

            var exception = new ApplicationException("Failed");
            var supervised = new StageSupervisedActor<IFailureControl>(FailureControlActor.Instance.Value, exception);

            var access = failureControlTestResults.AfterCompleting(1);

            supervised.Suspend();
            Assert.True(IsSuspended(FailureControlActor.Instance.Value));

            failure.AfterFailure(); // into suspended stowage
            supervised.Resume(); // sent

            Assert.Equal(1, access.ReadFromExpecting("afterFailureCount", 1));
        }

        [Fact]
        public void TestStopOne()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), "failure"));
            var exception = new ApplicationException("Failed");
            var supervised = new StageSupervisedActor<IFailureControl>(FailureControlActor.Instance.Value, exception);

            var access = failureControlTestResults.AfterCompleting(1);

            supervised.Stop(SupervisionStrategyConstants.Scope.One);

            Assert.Equal(1, access.ReadFromExpecting("stoppedCount", 1));
        }

        [Fact]
        public void TestStopAll()
        {
            var pingTestResults = new PingActor.PingTestResults();
            World.ActorFor<IPing>(Definition.Has<PingActor>(Definition.Parameters(pingTestResults), "ping"));

            var pongTestResults = new PongActor.PongTestResults();
            World.ActorFor<IPong>(Definition.Has<PongActor>(Definition.Parameters(pongTestResults), "pong"));

            var pingAccess = pingTestResults.AfterCompleting(1);
            var pongAccess = pongTestResults.AfterCompleting(1);

            var supervised = new StageSupervisedActor<IPing>(PingActor.Instance.Value, new ApplicationException("Failed"));
            supervised.Stop(SupervisionStrategyConstants.Scope.All);

            Assert.Equal(1, pingAccess.ReadFromExpecting("stopCount", 1));
            Assert.Equal(1, pongAccess.ReadFromExpecting("stopCount", 1));
        }
    }
}
