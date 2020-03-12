// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class PropertiesFileConfigRingBufferMailboxActorTest : ActorsTest
    {
        [Fact]
        public void TestThatRingBufferIsUsed()
        {
            var results = new TestResults(1);
            var one = World.ActorFor<IOneBehavior>(
                Definition.Has<OneBehaviorActor>(Definition.Parameters(results), "ringMailbox", "one-behavior"));

            one.DoSomething();

            Assert.Equal(1, results.Times);
        }

        public class OneBehaviorActor : Actor, IOneBehavior
        {
            private readonly TestResults results;

            public OneBehaviorActor(TestResults results)
            {
                this.results = results;
            }

            public void DoSomething() => results.Invoked();
        }

        public class TestResults
        {
            private readonly AccessSafely safely;

            public TestResults(int happenings)
            {
                var times = new AtomicInteger(0);
                safely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("times", _ => times.IncrementAndGet())
                    .ReadingWith("times", times.Get);
            }

            public int Times
            {
                get => safely.ReadFrom<int>("times");
            }

            public void Invoked() => safely.WriteUsing("times", 1);
        }
    }

    public interface IOneBehavior
    {
        void DoSomething();
    }
}
