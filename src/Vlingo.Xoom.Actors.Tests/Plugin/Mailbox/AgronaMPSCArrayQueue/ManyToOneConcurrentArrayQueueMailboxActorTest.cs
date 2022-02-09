// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Plugin;
using Vlingo.Xoom.Actors.Plugin.Completes;
using Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueMailboxActorTest : ActorsTest
    {
        private const int MailboxSize = 64;
        private const int MaxCount = 1024;

        public ManyToOneConcurrentArrayQueueMailboxActorTest()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.testArrayQueueMailbox", "true");
            properties.SetProperty("plugin.testArrayQueueMailbox.classname", "Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue.ManyToOneConcurrentArrayQueuePlugin");
            properties.SetProperty("plugin.testArrayQueueMailbox.defaultMailbox", "false");
            properties.SetProperty("plugin.testArrayQueueMailbox.size", "" + MailboxSize);
            properties.SetProperty("plugin.testArrayQueueMailbox.fixedBackoff", "2");
            properties.SetProperty("plugin.testArrayQueueMailbox.dispatcherThrottlingCount", "1");
            properties.SetProperty("plugin.testArrayQueueMailbox.sendRetires", "10");

            var provider = new ManyToOneConcurrentArrayQueuePlugin();
            var pluginProperties = new PluginProperties("testRingMailbox", properties);
            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);

            provider.Start(World);
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var testResults = new TestResults(MaxCount);
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-1"));
            const int totalCount = MailboxSize / 2;

            for (var count = 1; count <= totalCount; ++count)
            {
                countTaker.Take(count);
            }

            Assert.Equal(MaxCount, testResults.GetHighest());
        }

        [Fact]
        public void TestOverflowDispatch()
        {
            var testResults = new TestResults(MaxCount);
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-2"));
            const int totalCount = MailboxSize * 2;
            for (var count = 1; count <= totalCount; ++count)
            {
                countTaker.Take(count);
            }

            Assert.Equal(MaxCount, testResults.GetHighest());
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private ICountTaker self;
            public TestResults TestResults { get; }

            public CountTakerActor(TestResults testResults)
            {
                TestResults = testResults;
                self = SelfAs<ICountTaker>();
            }

            public void Take(int count)
            {
                if (TestResults.IsHighest(count))
                {
                    TestResults.SetHighest(count);
                }

                if (count < MaxCount)
                {
                    self.Take(count + 1);
                }
            }
        }

        private class TestResults
        {
            private readonly AccessSafely safely;

            public TestResults(int happenings)
            {
                var highest = new AtomicInteger(0);
                safely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("highest", x => highest.Set(x))
                    .ReadingWith("highest", highest.Get)
                    .ReadingWith<int, bool>("isHighest", count => count > highest.Get());
            }

            public void SetHighest(int value) => safely.WriteUsing("highest", value);

            public int GetHighest() => safely.ReadFrom<int>("highest");

            public bool IsHighest(int val) => safely.ReadFromNow<int, bool>("isHighest", val);
        }
    }
}
