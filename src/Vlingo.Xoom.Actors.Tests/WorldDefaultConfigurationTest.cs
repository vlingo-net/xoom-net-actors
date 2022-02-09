// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Actors.Tests
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
            private readonly WorldTest.TestResults _testResults;

            public SimpleActor(WorldTest.TestResults testResults)
            {
                _testResults = testResults;
            }

            public void SimplySay() => _testResults.SetInvoked(true);
        }
    }

    public interface ISimpleWorldForDefaultConfig
    {
        void SimplySay();
    }
}
