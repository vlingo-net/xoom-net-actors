// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxTest : ActorsTest
    {
        private const int Total = 10_000;

        private IDispatcher dispatcher;
        private IMailbox mailbox;

        public ConcurrentQueueMailboxTest()
        {
            dispatcher = new ExecutorDispatcher(1, 1.0f);
            mailbox = new ConcurrentQueueMailbox(dispatcher, 1);
        }

        public override void Dispose()
        {
            base.Dispose();
            mailbox.Close();
            dispatcher.Close();
        }

        [Fact]
        public void TestMailboxSendReceive()
        {
            var testResults = new TestResults();
            var actor = new CountTakerActor(testResults);
            actor.TestResults.Until = Until(Total);

            for (var i = 0; i < Total; ++i)
            {
                var countParam = i;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }
            actor.TestResults.Until.Completes();

            for(var i=0; i<Total; ++i)
            {
                Assert.Equal(i, actor.TestResults.Counts[i]);
            }
        }

        [Fact]
        public void TestSuspendedDelivery()
        {
            const string Paused = "paused#";
            const string Exceptional = "exceptional#";

            var dispatcher = new ExecutorDispatcher(1, 1.0f);
            var mailbox = new ConcurrentQueueMailbox(dispatcher, 1);

            var testResults = new TestResults();
            var actor = new CountTakerActor(testResults);

            mailbox.SuspendExceptFor(Paused, typeof(CountTakerActor));

            mailbox.SuspendExceptFor(Exceptional, typeof(CountTakerActor));

            mailbox.Resume(Exceptional);

            mailbox.Resume(Paused);

            Assert.False(mailbox.IsSuspended);
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            public CountTakerActor(TestResults testResults)
            {
                TestResults = testResults;
            }

            public TestResults TestResults { get; }

            public void Take(int count)
            {
                TestResults.Counts.Add(count);
                TestResults.Until.Happened();
            }
        }

        private class TestResults
        {
            public IList<int> Counts = new List<int>();
            public TestUntil Until = TestUntil.Happenings(0);
        }
    }
}
