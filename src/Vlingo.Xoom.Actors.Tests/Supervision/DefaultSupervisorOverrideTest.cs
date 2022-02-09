// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Actors.Plugin.Supervision;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Supervision
{
    public class DefaultSupervisorOverrideTest : ActorsTest
    {
        public DefaultSupervisorOverrideTest()
        {
            var configuration = Configuration.Define()
                .With(ConsoleLoggerPluginConfiguration.Define()
                    .WithDefaultLogger()
                    .WithName("vlingo-net/actors"))
                .With(DefaultSupervisorOverridePluginConfiguration.Define()
                    .WithSupervisor("default", "overrideSupervisor", typeof(DefaultSupervisorOverride)));

            TestWorld.Terminate();
            TestWorld = TestWorld.Start($"{typeof(DefaultSupervisorOverrideTest).Name}-world", configuration);
            World = TestWorld.World;
        }

        [Fact]
        public void TestOverride()
        {
            var testResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(testResults), "failure-for-stop"));

            var access = testResults.AfterCompleting(40);

            for (var idx = 0; idx < 20; ++idx)
            {
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            access.ReadFromExpecting("beforeResume", 20);
            Assert.Equal(20, access.ReadFrom<int>("beforeResume"));
            Assert.Equal(20, access.ReadFrom<int>("failNowCount"));
            Assert.Equal(20, access.ReadFrom<int>("afterFailureCount"));

            access = testResults.AfterCompleting(40);

            for (var idx = 0; idx < 20; ++idx)
            {
                failure.Actor.FailNow();
                failure.Actor.AfterFailure();
            }

            access.ReadFromExpecting("beforeResume", 40);
            Assert.Equal(40, access.ReadFrom<int>("failNowCount"));
            Assert.Equal(40, access.ReadFrom<int>("afterFailureCount"));

            Assert.False(failure.ActorInside.IsStopped);
        }
    }
}
