// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Xoom.Common;
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
            private readonly AtomicBoolean _receivedBeforeStart = new AtomicBoolean(false);
            private readonly AtomicBoolean _receivedAfterStop = new AtomicBoolean(false);
            internal readonly AccessSafely Received;

            private TestResults(AccessSafely received)
            {
                this.Received = received;
            }

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.Received.WritingWith<bool>("receivedBeforeStart", testResults._receivedBeforeStart.Set);
                testResults.Received.ReadingWith("receivedBeforeStart", testResults._receivedBeforeStart.Get);
                testResults.Received.WritingWith<bool>("receivedAfterStop", testResults._receivedAfterStop.Set);
                testResults.Received.ReadingWith("receivedAfterStop", testResults._receivedAfterStop.Get);
                return testResults;
            }

            public bool GetReceivedBeforeStart() => Received.ReadFrom<bool>("receivedBeforeStart");

            public bool GetReceivedAfterStop() => Received.ReadFrom<bool>("receivedAfterStop");
        }

        private class LifecycleActor : Actor, IStoppable
        {
            private readonly TestResults _testResults;

            public LifecycleActor(TestResults testResults)
            {
                this._testResults = testResults;
            }

            protected internal override void BeforeStart()
            {
                _testResults.Received.WriteUsing("receivedBeforeStart", true);
            }

            protected internal override void AfterStop()
            {
                _testResults.Received.WriteUsing("receivedAfterStop", true);
            }
        }
    }
}
