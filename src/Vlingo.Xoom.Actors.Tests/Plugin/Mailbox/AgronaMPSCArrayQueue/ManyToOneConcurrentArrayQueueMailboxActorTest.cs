// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
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

            var manyToOneConcurrentArrayQueuePlugin = new ManyToOneConcurrentArrayQueuePlugin();
            var pluginProperties = new PluginProperties("testArrayQueueMailbox", properties);
            var pooledCompletesPlugin = new PooledCompletesPlugin();
            
            pooledCompletesPlugin.Configuration.BuildWith(World.Configuration, pluginProperties);
            pooledCompletesPlugin.Start(World);

            manyToOneConcurrentArrayQueuePlugin.Configuration.BuildWith(World.Configuration, pluginProperties);
            manyToOneConcurrentArrayQueuePlugin.Start(World);
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var testResults = new TestResults(MaxCount);
            var countTaker = World.ActorFor<ICountTaker>(
                Definition.Has<CountTakerActor>(
                    Definition.Parameters(testResults), "testArrayQueueMailbox", "countTaker-1"));
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
                    Definition.Parameters(testResults), "testArrayQueueMailbox", "countTaker-2"));

            Assert.Throws<InvalidOperationException>(() =>
                {
                    for (var count = 1; count <= MailboxSize + 1; ++count)
                    {
                        countTaker.Take(count);
                    }
                }
            );
        }
        
        [Fact]
        public void TestMailboxIsConfigured()
        {
            var testResults = new TestResults(MaxCount);
            var countTaker =
                World.ActorFor<ICountTaker>(
                    Definition.Has<CountTakerActor>(
                        Definition.Parameters(testResults),
                        "testArrayQueueMailbox",
                        "countTaker"));

            var setMailboxTypeName = World.Stage.MailboxTypeNameOf(countTaker);
            Assert.Equal("ManyToOneConcurrentArrayQueueMailbox", setMailboxTypeName);
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly ICountTaker _self;
            public TestResults TestResults { get; }

            public CountTakerActor(TestResults testResults)
            {
                TestResults = testResults;
                _self = SelfAs<ICountTaker>();
            }

            public void Take(int count)
            {
                if (TestResults.IsHighest(count))
                {
                    TestResults.SetHighest(count);
                }

                if (count < MaxCount)
                {
                    _self.Take(count + 1);
                }
            }
        }

        private class TestResults
        {
            private readonly AccessSafely _safely;

            public TestResults(int happenings)
            {
                var highest = new AtomicInteger(0);
                _safely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("highest", x => highest.Set(x))
                    .ReadingWith("highest", highest.Get)
                    .ReadingWith<int, bool>("isHighest", count => count > highest.Get());
            }

            public void SetHighest(int value) => _safely.WriteUsing("highest", value);

            public int GetHighest() => _safely.ReadFrom<int>("highest");

            public bool IsHighest(int val) => _safely.ReadFromNow<int, bool>("isHighest", val);
        }
    }
}
