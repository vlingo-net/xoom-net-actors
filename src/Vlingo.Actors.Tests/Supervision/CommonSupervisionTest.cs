// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
            
            var supervisorResults = PingSupervisorActor.Instance.Value.TestResults;
            var pingAccess = testResults.AfterCompleting(5);
            var supervisorAccess = supervisorResults.AfterCompleting(5);


            for (var idx = 0; idx < 5; ++idx)
            {
                ping.Actor.Ping();
            }

            Assert.False(ping.ActorInside.IsStopped);
            Assert.Equal(5, pingAccess.ReadFrom<int>("pingCount"));
            Assert.Equal(5, supervisorAccess.ReadFrom<int>("informedCount"));

            pingAccess = testResults.AfterCompleting(2);

            ping.Actor.Ping();

            Assert.Equal(6, pingAccess.ReadFrom<int>("pingCount"));
            Assert.Equal(6, supervisorAccess.ReadFrom<int>("informedCount"));

            Assert.True(ping.ActorInside.IsStopped);
        }

        [Fact]
        public void TestPongSupervisor()
        {
            var testResults = new PongActor.PongTestResults();
            var pong = TestWorld.ActorFor<IPong>(
                Definition.Has<PongActor>(
                    Definition.Parameters(testResults), "pong"));

            var supervisorResults = PongSupervisorActor.Instance.Value.TestResults;
            var pongAccess = testResults.AfterCompleting(10);
            var supervisorAccess = supervisorResults.AfterCompleting(10);

            for (var idx = 0; idx < 10; ++idx)
            {
                pong.Actor.Pong();
            }

            Assert.Equal(10, pongAccess.ReadFrom<int>("pongCount"));
            Assert.Equal(10, supervisorAccess.ReadFrom<int>("informedCount"));

            Assert.False(pong.ActorInside.IsStopped);

            pongAccess = testResults.AfterCompleting(2);

            pong.Actor.Pong();

            Assert.Equal(11, pongAccess.ReadFrom<int>("pongCount"));
            Assert.Equal(11, supervisorAccess.ReadFrom<int>("informedCount"));

            Assert.True(pong.ActorInside.IsStopped);
        }
    }
}
