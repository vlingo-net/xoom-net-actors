// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferMailboxActorTest : ActorsTest
    {
        private const int MailboxSize = 64;
        private const int MaxCount = 1024;

        public RingBufferMailboxActorTest()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.testRingMailbox", "true");
            properties.SetProperty("plugin.testRingMailbox.classname", "Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer.SharedRingBufferMailboxPlugin");
            properties.SetProperty("plugin.testRingMailbox.defaultMailbox", "false");
            properties.SetProperty("plugin.testRingMailbox.size", $"{MailboxSize}");
            properties.SetProperty("plugin.testRingMailbox.fixedBackoff", "2");
            properties.SetProperty("plugin.testRingMailbox.numberOfDispatchersFactor", "1.0");
            properties.SetProperty("plugin.testRingMailbox.dispatcherThrottlingCount", "10");

            var provider = new SharedRingBufferMailboxPlugin();
            var pluginProperties = new PluginProperties("testRingMailbox", properties);
            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);

            provider.Start(World);
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var testResults = new TestResults();
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-1"));
            var totalCount = MailboxSize / 2;
            testResults.Until = Until(MaxCount);

            for (var count = 1; count <= totalCount; ++count)
            {
                countTaker.Take(count);
            }
            testResults.Until.Completes();

            Assert.Equal(MaxCount, testResults.Highest.Get());
        }

        [Fact]
        public void TestOverflowDispatch()
        {
            var testResults = new TestResults();
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-2"));
            var totalCount = MailboxSize * 2;
            testResults.Until = Until(MaxCount);

            for (var count = 1; count <= totalCount; ++count)
            {
                countTaker.Take(count);
            }
            testResults.Until.Completes();

            Assert.Equal(MaxCount, testResults.Highest.Get());
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
                if (count > testResults.Highest.Get())
                {
                    testResults.Highest.Set(count);
                    testResults.Until.Happened();
                }
                if (count < MaxCount)
                {
                    self.Take(count + 1);
                }
                else
                {
                    testResults.Until.CompleteNow();
                }
            }
        }

        private class TestResults
        {
            public AtomicInteger Highest = new AtomicInteger(0);
            public TestUntil Until = TestUntil.Happenings(0);
        }
    }
}
