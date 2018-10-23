// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Tests.Supervision;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class StageSupervisedActorTest : ActorsTest
    {
        [Fact]
        public void TestExpectedAttributes()
        {
            var testResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(testResults), "failure"));
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

            supervised.Escalate();

            Assert.Equal(1, failureControlTestResults.StoppedCount.Get());
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

            supervised.RestartWithin(1000, 5, SupervisionStrategyConstants.Scope.One);

            Assert.Equal(2, failureControlTestResults.BeforeStartCount.Get());
            Assert.Equal(1, failureControlTestResults.BeforeRestartCount.Get());
            Assert.Equal(1, failureControlTestResults.AfterRestartCount.Get());
        }

        [Fact]
        public void TestSuspendResume()
        {
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), "failure"));
            failureControlTestResults.UntilAfterFail = Until(1);
            var exception = new ApplicationException("Failed");
            var supervised = new StageSupervisedActor<IFailureControl>(FailureControlActor.Instance.Value, exception);

            supervised.Suspend();
            Assert.True(IsSuspended(FailureControlActor.Instance.Value));

            failure.AfterFailure(); // into suspended stowage
            supervised.Resume(); // sent
            failureControlTestResults.UntilAfterFail.Completes(); // delivered

            Assert.Equal(1, failureControlTestResults.AfterFailureCount.Get());
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
            failureControlTestResults.UntilStopped = Until(1);

            supervised.Stop(SupervisionStrategyConstants.Scope.One);
            failureControlTestResults.UntilStopped.Completes();

            Assert.True(FailureControlActor.Instance.Value.IsStopped);
        }

        [Fact]
        public void TestStopAll()
        {
            var pingTestResults = new PingActor.PingTestResults();
            World.ActorFor<IPing>(Definition.Has<PingActor>(Definition.Parameters(pingTestResults), "ping"));

            var pongTestResults = new PongActor.PongTestResults();
            World.ActorFor<IPong>(Definition.Has<PongActor>(Definition.Parameters(pongTestResults), "pong"));

            pingTestResults.UntilStopped = Until(1);
            pongTestResults.UntilStopped = Until(1);

            var supervised = new StageSupervisedActor<IPing>(PingActor.Instance.Value, new ApplicationException("Failed"));
            supervised.Stop(SupervisionStrategyConstants.Scope.All);

            pingTestResults.UntilStopped.Completes();
            pongTestResults.UntilStopped.Completes();

            Assert.True(PingActor.Instance.Value.IsStopped);
            Assert.True(PongActor.Instance.Value.IsStopped);
        }
    }
}
