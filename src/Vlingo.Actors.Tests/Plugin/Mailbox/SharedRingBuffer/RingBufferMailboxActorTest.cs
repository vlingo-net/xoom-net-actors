// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferMailboxActorTest : ActorsTest
    {
        private const int MailboxSize = 64;
        private const int MaxCount = 1024;

        private const int ThroughputMailboxSize = 1048576;
        private const int ThroughputMaxCount = 4194304; // 104857600;
        private const int ThroughputWarmUpCount = 4194304;

        [Fact]
        public void TestBasicDispatch()
        {
            Init(MailboxSize);
            var testResults = new TestResults();
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-1"));
            var totalCount = MailboxSize / 2;
            testResults.Until = Until(MaxCount);
            testResults.maximum = MaxCount;

            for (var count = 1; count <= totalCount; ++count)
            {
                countTaker.Take(count);
            }
            testResults.Until.Completes();

            Assert.Equal(MaxCount, testResults.highest);
        }

        [Fact]
        public void TestThroughput()
        {
            Init(ThroughputMailboxSize);

            var testResults = new TestResults();
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<ThroughputCountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-2"));

            testResults.maximum = ThroughputWarmUpCount;

            for (var count = 1; count <= ThroughputWarmUpCount; ++count)
            {
                countTaker.Take(count);
            }

            while (testResults.highest < ThroughputWarmUpCount) { }

            testResults.highest = 0;
            testResults.maximum = ThroughputMaxCount;

            var startTime = DateTime.UtcNow;

            for (int count = 1; count <= ThroughputMaxCount; ++count)
            {
                countTaker.Take(count);
            }

            while (testResults.highest < ThroughputMaxCount) { }

            var timeSpent = DateTime.UtcNow - startTime;

            Console.WriteLine("Ms: " + timeSpent.TotalMilliseconds + " FOR " + ThroughputMaxCount + " MESSAGES IS " + (ThroughputMaxCount / timeSpent.TotalSeconds) + " PER SECOND");

            Assert.Equal(ThroughputMaxCount, testResults.highest);
        }

        private void Init(int mailboxSize)
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.testRingMailbox", "true");
            properties.SetProperty("plugin.testRingMailbox.classname", "Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer.SharedRingBufferMailboxPlugin");
            properties.SetProperty("plugin.testRingMailbox.defaultMailbox", "false");
            properties.SetProperty("plugin.testRingMailbox.size", $"{mailboxSize}");
            properties.SetProperty("plugin.testRingMailbox.fixedBackoff", "2");
            properties.SetProperty("plugin.testRingMailbox.numberOfDispatchersFactor", "1.0");
            properties.SetProperty("plugin.testRingMailbox.dispatcherThrottlingCount", "20");

            var provider = new SharedRingBufferMailboxPlugin();
            var pluginProperties = new PluginProperties("testRingMailbox", properties);
            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);

            provider.Start(World);
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private ICountTaker self;
            private readonly TestResults testResults;

            public CountTakerActor(TestResults testResults)
            {
                this.testResults = testResults;
                self = SelfAs<ICountTaker>();
            }

            public void Take(int count)
            {
                if (count > testResults.highest)
                {
                    testResults.highest = count;
                    testResults.Until.Happened();
                }
                if (count < testResults.maximum)
                {
                    self.Take(count + 1);
                }
                else
                {
                    testResults.Until.CompleteNow();
                }
            }
        }

        private class ThroughputCountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults testResults;

            public ThroughputCountTakerActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            public void Take(int count)
            {
                testResults.highest = count;
            }
        }

        private class TestResults
        {
            public volatile int highest = 0;
            public TestUntil Until = TestUntil.Happenings(0);
            public int maximum = 0;
        }
    }
}
