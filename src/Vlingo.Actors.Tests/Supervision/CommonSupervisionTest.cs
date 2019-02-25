// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Supervision;
using Xunit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class CommonSupervisionTest : ActorsTest
    {
        public CommonSupervisionTest()
        {
            var configuration = Configuration.Define()
                .With(
                    ConsoleLoggerPluginConfiguration.Define()
                    .WithDefaultLogger()
                    .WithName("vlingo-net/actors"))
                .With(
                    CommonSupervisorsPluginConfiguration.Define()
                    .WithSupervisor("default", "pingSupervisor", typeof(IPing), typeof(PingSupervisorActor))
                    .WithSupervisor("default", "pongSupervisor", typeof(IPong), typeof(PongSupervisorActor))
                );

            TestWorld.Terminate();
            TestWorld = Actors.TestKit.TestWorld.Start($"{typeof(CommonSupervisionTest).Name}-world", configuration);
            World = TestWorld.World;
        }

        [Fact]
        public void TestPingSupervisor()
        {
            var testResults = new PingActor.PingTestResults();
            var ping = TestWorld.ActorFor<IPing>(
                Definition.Has<PingActor>(
                    Definition.Parameters(testResults), "ping"));
            testResults.UntilPinged = Until(5);

            for (var idx = 0; idx < 5; ++idx)
            {
                World.DefaultLogger.Log("PingSupervisorActor instance: " + PingSupervisorActor.Instance.Value);
                World.DefaultLogger.Log("PingSupervisorActor testResults: " + PingSupervisorActor.Instance.Value.TestResults);
                World.DefaultLogger.Log("PingSupervisorActor testResults untilInform: " + PingSupervisorActor.Instance.Value.TestResults.UntilInform);

                PingSupervisorActor.Instance.Value.TestResults.UntilInform = Until(1);
                ping.Actor.Ping();
                PingSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();
            }

            testResults.UntilPinged.Completes();
            // PingSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();

            Assert.False(ping.ActorInside.IsStopped);
            Assert.Equal(5, testResults.PingCount.Get());
            Assert.Equal(5, PingSupervisorActor.Instance.Value.TestResults.InformedCount.Get());

            testResults.UntilPinged = Until(1);
            testResults.UntilStopped = Until(1);
            PingSupervisorActor.Instance.Value.TestResults.UntilInform = Until(1);

            ping.Actor.Ping();

            PingSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();
            testResults.UntilPinged.Completes();
            testResults.UntilStopped.Completes();

            Assert.True(ping.ActorInside.IsStopped);
            Assert.Equal(6, testResults.PingCount.Get());
            Assert.Equal(6, PingSupervisorActor.Instance.Value.TestResults.InformedCount.Get());
        }

        [Fact]
        public void TestPongSupervisor()
        {
            var testResults = new PongActor.PongTestResults();
            var pong = TestWorld.ActorFor<IPong>(
                Definition.Has<PongActor>(
                    Definition.Parameters(testResults), "pong"));
            testResults.UntilPonged = Until(5);

            for (var idx = 0; idx < 10; ++idx)
            {
                PongSupervisorActor.Instance.Value.TestResults.UntilInform = Until(1);
                pong.Actor.Pong();
                PongSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();
            }

            testResults.UntilPonged.Completes();
            // PongSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();

            Assert.False(pong.ActorInside.IsStopped);
            Assert.Equal(10, testResults.PongCount.Get());
            Assert.Equal(10, PongSupervisorActor.Instance.Value.TestResults.InformedCount.Get());

            testResults.UntilPonged = Until(1);
            testResults.UntilStopped = Until(1);
            PongSupervisorActor.Instance.Value.TestResults.UntilInform = Until(1);

            pong.Actor.Pong();

            PongSupervisorActor.Instance.Value.TestResults.UntilInform.Completes();
            testResults.UntilPonged.Completes();
            testResults.UntilStopped.Completes();

            Assert.True(pong.ActorInside.IsStopped);
            Assert.Equal(11, testResults.PongCount.Get());
            Assert.Equal(11, PongSupervisorActor.Instance.Value.TestResults.InformedCount.Get());
        }
    }
}
