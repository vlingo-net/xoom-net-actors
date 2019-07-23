// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Actors.Tests
{
    public class WorldDefaultConfigurationTest
    {
        [Fact]
        public void TestStartWorldWithDefaultConfiguration()
        {
            var worldDefaultConfig = World.Start("defaults");
            var testResults = new WorldTest.TestResults(1);
            var simple = worldDefaultConfig.ActorFor<ISimpleWorldForDefaultConfig>(
                Definition.Has<SimpleActor>(
                    Definition.Parameters(testResults)));

            simple.SimplySay();

            Assert.True(testResults.Invoked);
        }

        private class SimpleActor : Actor, ISimpleWorldForDefaultConfig
        {
            private readonly WorldTest.TestResults testResults;

            public SimpleActor(WorldTest.TestResults testResults)
            {
                this.testResults = testResults;
            }

            public void SimplySay() => testResults.SetInvoked(true);
        }
    }

    public interface ISimpleWorldForDefaultConfig
    {
        void SimplySay();
    }
}
