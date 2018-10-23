// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

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

            failureControlTestResults.UntilFailNow = Until(20);
            failureControlTestResults.UntilAfterFail = Until(20);
            for(var idx=1; idx<=20; ++idx)
            {
                resumeForeverSupervisorTestResults.UntilInform = Until(1);
                failure.Actor.FailNow();
                resumeForeverSupervisorTestResults.UntilInform.Completes();
                failure.Actor.AfterFailure();
            }
            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            failureControlTestResults.UntilFailNow = Until(20);
            failureControlTestResults.UntilAfterFail = Until(20);
            for (var idx = 1; idx <= 20; ++idx)
            {
                resumeForeverSupervisorTestResults.UntilInform = Until(1);
                failure.Actor.FailNow();
                resumeForeverSupervisorTestResults.UntilInform.Completes();
                failure.Actor.AfterFailure();
            }
            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            Assert.Equal(40, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(40, failureControlTestResults.AfterFailureCount.Get());
            Assert.True(40 <= resumeForeverSupervisorTestResults.InformedCount.Get());
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

            failureControlTestResults.UntilFailNow = Until(20);
            failureControlTestResults.UntilAfterFail = Until(20);
            for (var idx = 1; idx <= 20; ++idx)
            {
                restartForeverSupervisorTestResults.UntilInform = Until(1);
                failure.Actor.FailNow();
                restartForeverSupervisorTestResults.UntilInform.Completes();
                failure.Actor.AfterFailure();
            }
            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            failureControlTestResults.UntilFailNow = Until(20);
            failureControlTestResults.UntilAfterFail = Until(20);
            for (var idx = 1; idx <= 20; ++idx)
            {
                restartForeverSupervisorTestResults.UntilInform = Until(1);
                failure.Actor.FailNow();
                restartForeverSupervisorTestResults.UntilInform.Completes();
                failure.Actor.AfterFailure();
            }
            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            Assert.Equal(40, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(40, failureControlTestResults.AfterFailureCount.Get());
            Assert.True(40 <= restartForeverSupervisorTestResults.InformedCount.Get());
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

            failureControlTestResults.UntilFailNow = Until(5);
            failureControlTestResults.UntilAfterFail = Until(5);

            for(var idx=1; idx <= 5; ++idx)
            {
                restartFiveInOneSupervisorTestResults.UntilInform = Until(1);
                failure.Actor.FailNow();
                restartFiveInOneSupervisorTestResults.UntilInform.Completes();
                failure.Actor.AfterFailure();
            }
            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            Assert.Equal(5, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(5, failureControlTestResults.AfterFailureCount.Get());

            failureControlTestResults.UntilFailNow = Until(1);
            failureControlTestResults.UntilAfterFail = Until(0);
            restartFiveInOneSupervisorTestResults.UntilInform = Until(1);

            failure.Actor.FailNow();
            failure.Actor.AfterFailure();

            failureControlTestResults.UntilFailNow.Completes();
            restartFiveInOneSupervisorTestResults.UntilInform.Completes();
            failureControlTestResults.UntilAfterFail.Completes();

            Assert.True(failure.ActorInside.IsStopped);
            Assert.Equal(6, failureControlTestResults.FailNowCount.Get());
            Assert.Equal(6, restartFiveInOneSupervisorTestResults.InformedCount.Get());
            Assert.Equal(5, failureControlTestResults.AfterFailureCount.Get());
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

            failureControlTestResults.UntilFailNow = Until(1);
            failureControlTestResults.UntilAfterFail = Until(1);

            Assert.Equal(0, escalateSupervisorTestResults.InformedCount.Get());
            Assert.Equal(0, failureControlTestResults.StoppedCount.Get());

            failure.Actor.FailNow();

            failureControlTestResults.UntilFailNow.Completes();
            failureControlTestResults.UntilStopped.Completes();

            Assert.Equal(1, escalateSupervisorTestResults.InformedCount.Get());
            Assert.Equal(1, failureControlTestResults.StoppedCount.Get());
        }

        [Fact]
        public void TestStopAll()
        {
            World.ActorFor<ISupervisor>(Definition.Has<StopAllSupervisorActor>(Definition.NoParameters, "stop-all"));

            var pingTestResults = new PingActor.PingTestResults();
            var ping = World.ActorFor<IPing>(
                Definition.Has<PingActor>(
                    Definition.Parameters(pingTestResults), StopAllSupervisorActor.Instance.Value, "ping"));
            var pongTestResults = new PongActor.PongTestResults();
            var pong = World.ActorFor<IPong>(
                Definition.Has<PongActor>(
                    Definition.Parameters(pongTestResults), StopAllSupervisorActor.Instance.Value, "pong"));
            pingTestResults.UntilStopped = Until(1);
            pongTestResults.UntilStopped = Until(1);

            Assert.False(PingActor.Instance.Value.IsStopped);
            Assert.False(PongActor.Instance.Value.IsStopped);

            ping.Ping();
            pingTestResults.UntilStopped.Completes();
            pongTestResults.UntilStopped.Completes();

            Assert.True(PingActor.Instance.Value.IsStopped);
            Assert.True(PongActor.Instance.Value.IsStopped);
        }
    }
}
