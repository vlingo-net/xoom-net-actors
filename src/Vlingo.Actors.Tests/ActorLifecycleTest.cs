// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
            var testResults = TestResults.AfterCompleting(1);
            var actor = World.ActorFor<IStoppable>(typeof(LifecycleActor), testResults);

            Assert.True(testResults.GetReceivedBeforeStart());
            Assert.False(testResults.GetReceivedAfterStop());
        }

        [Fact]
        public void TestAfterStop()
        {
            var testResults = TestResults.AfterCompleting(2);

            var actor = World.ActorFor<IStoppable>(typeof(LifecycleActor), testResults);
            actor.Stop();

            Assert.True(testResults.GetReceivedBeforeStart());
            Assert.True(testResults.GetReceivedAfterStop());
        }

        private class TestResults
        {
            private readonly AtomicBoolean receivedBeforeStart = new AtomicBoolean(false);
            private readonly AtomicBoolean receivedAfterStop = new AtomicBoolean(false);
            internal readonly AccessSafely received;

            private TestResults(AccessSafely received)
            {
                this.received = received;
            }

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.received.WritingWith<bool>("receivedBeforeStart", testResults.receivedBeforeStart.Set);
                testResults.received.ReadingWith("receivedBeforeStart", testResults.receivedBeforeStart.Get);
                testResults.received.WritingWith<bool>("receivedAfterStop", testResults.receivedAfterStop.Set);
                testResults.received.ReadingWith("receivedAfterStop", testResults.receivedAfterStop.Get);
                return testResults;
            }

            public bool GetReceivedBeforeStart() => received.ReadFrom<bool>("receivedBeforeStart");

            public bool GetReceivedAfterStop() => received.ReadFrom<bool>("receivedAfterStop");
        }

        private class LifecycleActor : Actor, IStoppable
        {
            private readonly TestResults testResults;

            public LifecycleActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            protected internal override void BeforeStart()
            {
                testResults.received.WriteUsing("receivedBeforeStart", true);
            }

            protected internal override void AfterStop()
            {
                testResults.received.WriteUsing("receivedAfterStop", true);
            }
        }
    }
}
