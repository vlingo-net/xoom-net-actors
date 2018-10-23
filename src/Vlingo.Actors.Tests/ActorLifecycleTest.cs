// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorLifecycleTest : ActorsTest
    {
        [Fact]
        public void TestBeforeStart()
        {
            var testResults = new TestResults();
            testResults.until = Until(2);
            var actor = World.ActorFor<IStoppable>(Definition.Has<LifecycleActor>(Definition.Parameters(testResults)));
            actor.Stop();
            testResults.until.Completes();

            Assert.True(testResults.receivedBeforeStart.Get());
            Assert.True(testResults.receivedAfterStop.Get());
        }

        private class TestResults
        {
            public AtomicBoolean receivedBeforeStart = new AtomicBoolean(false);
            public AtomicBoolean receivedAfterStop = new AtomicBoolean(false);
            public TestUntil until = TestUntil.Happenings(0);
        }

        private class LifecycleActor : Actor, IStoppable
        {
            private readonly TestResults testResults;

            public LifecycleActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            internal override void BeforeStart()
            {
                testResults.receivedBeforeStart.Set(true);
                testResults.until.Happened();
            }

            internal override void AfterStop()
            {
                testResults.receivedAfterStop.Set(true);
                testResults.until.Happened();
            }
        }
    }
}
