// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class WorldDefaultConfigurationTest
    {
        [Fact]
        public void TestStartWorldWithDefaultConfiguration()
        {
            var worldDefaultConfig = World.Start("defaults");
            var testResults = new TestResults();
            var simple = worldDefaultConfig.ActorFor<ISimpleWorldForDefaultConfig>(
                Definition.Has<SimpleActor>(
                    Definition.Parameters(testResults)));
            testResults.UntilSimple = TestUntil.Happenings(1);

            simple.SimplySay();
            testResults.UntilSimple.Completes();

            Assert.True(testResults.Invoked.Get());
        }

        private class SimpleActor : Actor, ISimpleWorldForDefaultConfig
        {
            private readonly TestResults testResults;

            public SimpleActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            public void SimplySay()
            {
                testResults.Invoked.Set(true);
                testResults.UntilSimple.Happened();
            }
        }

        private class TestResults
        {
            public AtomicBoolean Invoked { get; set; } = new AtomicBoolean(false);
            public TestUntil UntilSimple { get; set; } = TestUntil.Happenings(0);
        }
    }

    public interface ISimpleWorldForDefaultConfig
    {
        void SimplySay();
    }
}
