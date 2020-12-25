// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferMailboxActorTest : ActorsTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RingBufferMailboxActorTest(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        private const int MailboxSize = 64;
        private const int MaxCount = 1024;

        private const int ThroughputMailboxSize = 1048576;
        private const int ThroughputMaxCount = 4194304; // 104857600;
        private const int ThroughputWarmUpCount = 4194304;

        [Fact]
        public async Task TestBasicDispatch()
        {
            Init(MailboxSize);
            var testResults = new TestResults(MaxCount);
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-1"));
            
            testResults.SetMaximum(MaxCount);

            for (var count = 1; count <= MaxCount; ++count)
            {
                countTaker.Take(count);
            }

            await Task.Delay(10);

            Assert.Equal(MaxCount, testResults.GetHighest());
        }

        [Fact]
        public void TestThroughput()
        {
            Init(ThroughputMailboxSize);

            var testResults = new TestResults(ThroughputMailboxSize);
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<ThroughputCountTakerActor>(
                    Definition.Parameters(testResults), "testRingMailbox", "countTaker-2"));

            testResults.SetMaximum(ThroughputWarmUpCount);

            for (var count = 1; count <= ThroughputWarmUpCount; ++count)
            {
                countTaker.Take(count);
            }

            while (testResults.GetHighest() < ThroughputWarmUpCount) { }

            testResults.SetHighest(0);
            testResults.SetMaximum(ThroughputMaxCount);

            var startTime = DateTime.UtcNow;

            for (int count = 1; count <= ThroughputMaxCount; ++count)
            {
                countTaker.Take(count);
            }

            while (testResults.GetHighest() < ThroughputMaxCount) { }

            var timeSpent = DateTime.UtcNow - startTime;

            _testOutputHelper.WriteLine("Ms: {0} FOR {1} MESSAGES IS {2} PER SECOND", timeSpent.TotalMilliseconds, ThroughputMaxCount, ThroughputMaxCount / timeSpent.TotalSeconds);

            Assert.Equal(ThroughputMaxCount, testResults.GetHighest());
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
            private ICountTaker _self;
            private readonly TestResults _testResults;

            public CountTakerActor(TestResults testResults)
            {
                _testResults = testResults;
                _self = SelfAs<ICountTaker>();
            }

            public void Take(int count)
            {
                if (_testResults.IsHighest(count))
                {
                    _testResults.SetHighest(count);
                }
            }
        }

        private class ThroughputCountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults _testResults;

            public ThroughputCountTakerActor(TestResults testResults) => _testResults = testResults;

            public void Take(int count) => _testResults.SetHighest(count);
        }

        private class TestResults
        {
            private readonly AccessSafely _accessSafely;

            public TestResults(int happenings)
            {
                var highest = new AtomicInteger(0);
                var maximum = new AtomicInteger(0);
                _accessSafely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("highest", highest.Set)
                    .ReadingWith("highest", highest.Get)
                    .WritingWith<int>("maximum", maximum.Set)
                    .ReadingWith("maximum", maximum.Get)
                    .ReadingWith<int, bool>("isHighest", count => count > highest.Get());
            }

            public void SetHighest(int value) => _accessSafely.WriteUsing("highest", value);
            
            public void SetMaximum(int value) => _accessSafely.WriteUsing("maximum", value);

            public int GetHighest() => _accessSafely.ReadFrom<int>("highest");
            
            public int GetMaximum() => _accessSafely.ReadFrom<int>("maximum");

            public bool IsHighest(int value) => _accessSafely.ReadFromNow<int, bool>("isHighest", value);
        }
    }
}
