// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
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
            testResults.Until = Until(1);
            world.ActorFor(Definition.Has<LifecycleActor>(Definition.Parameters(testResults)), new Type[] { typeof(IStoppable) });
            testResults.Until.Completes();
            Assert.True(testResults.ReceivedBeforeStart.Get());
            Assert.False(testResults.ReceivedAfterStop.Get());
        }

        internal sealed class LifecycleActor: Actor, IStoppable 
        {
            private readonly TestResults testResults;

            internal LifecycleActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            internal override void BeforeStart()
            {
                testResults.ReceivedBeforeStart.Set(true);
                testResults.Until.Happened();
            }

            internal override void AfterStop()
            {
                testResults.ReceivedAfterStop.Set(true);
                testResults.Until.Happened();
            }
        }

        internal class TestResults 
        {
            public AtomicBoolean ReceivedBeforeStart { get; } = new AtomicBoolean(false);
            public AtomicBoolean ReceivedAfterStop { get; } = new AtomicBoolean(false);
            public TestUntil Until { get; set; } = TestUntil.Happenings(0);
        }
    }
}