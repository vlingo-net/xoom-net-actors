// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class PropertiesFileConfigRingBufferMailboxActorTest : ActorsTest
    {
        [Fact]
        public void TestThatRingBufferIsUsed()
        {
            var results = new TestResults();
            var one = World.ActorFor<IOneBehavior>(
                Definition.Has<OneBehaviorActor>(Definition.Parameters(results), "ringMailbox", "one-behavior"));

            one.DoSomething();

            results.until.Completes();

            Assert.Equal(1, results.times);
        }

        public class OneBehaviorActor : Actor, IOneBehavior
        {
            private readonly TestResults results;

            public OneBehaviorActor(TestResults results)
            {
                this.results = results;
            }

            public void DoSomething()
            {
                results.times++;
                results.until.Happened();
            }
        }

        public class TestResults
        {
            public volatile int times = 0;
            public TestUntil until = TestUntil.Happenings(1);
        }
    }

    public interface IOneBehavior
    {
        void DoSomething();
    }
}
