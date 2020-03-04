// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;
using StopAllSupervisorResult = Vlingo.Actors.Tests.Supervision.StopAllSupervisorActor.StopAllSupervisorResult;

namespace Vlingo.Actors.Tests.Supervision
{
    public class SupervisionStrategyTest : ActorsTest
    {
        [Fact]
        public void TestResumeForeverStrategy()
        {
            var resumeForeverSupervisorTestResults = new ResumeForeverSupervisorActor.ResumeForeverSupervisorTestResults();
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<ResumeForeverSupervisorActor>(
                    Definition.Parameters(resumeForeverSupervisorTestResults), "resume-forever-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure-for-stop"));

            var failureAccess = failureControlTestResults.AfterCompleting(0);
            var resumeAccess = resumeForeverSupervisorTestResults.AfterCompleting(1);

            for (var idx = 1; idx <= 20; ++idx)
            {
                failureAccess = failureControlTestResults.AfterCompleting(1);
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            Assert.Equal(20, failureAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(20, failureAccess.ReadFrom<int>("afterFailureCount"));
            Assert.Equal(20, resumeAccess.ReadFrom<int>("informedCount"));

            failureAccess = failureControlTestResults.AfterCompleting(20);

            for (var idx = 1; idx <= 20; ++idx)
            {
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            Assert.Equal(40, failureAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(40, failureAccess.ReadFrom<int>("afterFailureCount"));
            Assert.True(40 <= resumeAccess.ReadFrom<int>("informedCount"));
        }

        [Fact]
        public void TestRestartForeverStrategy()
        {
            var restartForeverSupervisorTestResults = new RestartForeverSupervisorActor.RestartForeverSupervisorTestResults();
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<RestartForeverSupervisorActor>(
                    Definition.Parameters(restartForeverSupervisorTestResults), "restart-forever-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure-for-stop"));

            var failedAccess = failureControlTestResults.AfterCompleting(40);
            var restartAccess = restartForeverSupervisorTestResults.AfterCompleting(40);

            for (var idx = 1; idx <= 20; ++idx)
            {
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            Assert.Equal(20, failedAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(20, failedAccess.ReadFrom<int>("afterFailureCount"));

            failedAccess = failureControlTestResults.AfterCompleting(40);

            for (var idx = 1; idx <= 20; ++idx)
            {
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            Assert.Equal(40, failedAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(40, failedAccess.ReadFrom<int>("afterFailureCount"));
            Assert.True(40 <= restartAccess.ReadFrom<int>("informedCount"));
        }

        [Fact]
        public void Test5Intensity1PeriodRestartStrategy()
        {
            var restartFiveInOneSupervisorTestResults = new RestartFiveInOneSupervisorActor.RestartFiveInOneSupervisorTestResults();
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<RestartFiveInOneSupervisorActor>(
                    Definition.Parameters(restartFiveInOneSupervisorTestResults), "resuming-5-1-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure-for-stop"));

            var failureAccess = failureControlTestResults.AfterCompleting(0);
            var restartAccess = restartFiveInOneSupervisorTestResults.AfterCompleting(5);

            for (var idx = 1; idx <= 5; ++idx)
            {
                failureAccess = failureControlTestResults.AfterCompleting(1);
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            Assert.Equal(5, failureAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(5, failureAccess.ReadFrom<int>("afterFailureCount"));

            failureAccess = failureControlTestResults.AfterCompleting(1);
            restartAccess = restartFiveInOneSupervisorTestResults.AfterCompleting(1);

            failure.Actor.FailNow();
            failure.Actor.AfterFailure();

            Assert.True(failure.ActorInside.IsStopped);
            Assert.Equal(6, failureAccess.ReadFrom<int>("failNowCount"));
            Assert.Equal(5, failureAccess.ReadFrom<int>("afterFailureCount"));
            Assert.Equal(6, restartAccess.ReadFrom<int>("informedCount"));
        }

        [Fact]
        public void TestEscalate()
        {
            var escalateSupervisorTestResults = new EscalateSupervisorActor.EscalateSupervisorTestResults();
            var supervisor = TestWorld.ActorFor<ISupervisor>(
                Definition.Has<EscalateSupervisorActor>(
                    Definition.Parameters(escalateSupervisorTestResults), "escalate"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults), supervisor.ActorInside, "failure"));

            var escalateAccess = escalateSupervisorTestResults.AfterCompleting(1);
            var failureAccess = failureControlTestResults.AfterCompleting(1);

            failure.Actor.FailNow();

            Assert.Equal(1, escalateAccess.ReadFrom<int>("informedCount"));
            Assert.Equal(1, failureAccess.ReadFrom<int>("stoppedCount"));
        }

        [Fact]
        public void TestStopAll()
        {
            var stopResults = new StopAllSupervisorResult();

            World.ActorFor<ISupervisor>(Definition.Has<StopAllSupervisorActor>(Definition.Parameters(stopResults), "stop-all"));

            var pingTestResults = new PingActor.PingTestResults();
            var ping = World.ActorFor<IPing>(
                Definition.Has<PingActor>(
                    Definition.Parameters(pingTestResults), StopAllSupervisorActor.Instance.Value, "ping"));
            var pongTestResults = new PongActor.PongTestResults();
            var pong = World.ActorFor<IPong>(
                Definition.Has<PongActor>(
                    Definition.Parameters(pongTestResults), StopAllSupervisorActor.Instance.Value, "pong"));

            var pingAccess = pingTestResults.AfterCompleting(1);
            var pongAccess = pongTestResults.AfterCompleting(1);
            var stopAccess = stopResults.AfterCompleting(1);

            ping.Ping();

            Assert.Equal(1, stopAccess.ReadFrom<int>("informedCount"));
            Assert.Equal(1, pingAccess.ReadFrom<int>("stopCount"));
            Assert.Equal(1, pongAccess.ReadFrom<int>("stopCount"));
        }
    }
}
