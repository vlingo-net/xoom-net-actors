// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxTest : ActorsTest
    {
        private const int Total = 10_000;

        private IDispatcher _dispatcher;
        private IMailbox _mailbox;

        public ConcurrentQueueMailboxTest()
        {
            _dispatcher = new ExecutorDispatcher(1, 0,1.0f);
            _mailbox = new ConcurrentQueueMailbox(_dispatcher, 1);
        }

        public override void Dispose()
        {
            base.Dispose();
            _mailbox.Close();
            _dispatcher.Close();
        }

        [Fact]
        public void TestMailboxSendReceive()
        {
            var testResults = new TestResults(Total);
            var actor = new CountTakerActor(testResults);

            for (var i = 0; i < Total; ++i)
            {
                var countParam = i;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                _mailbox.Send(message);
            }

            for (var i = 0; i < Total; ++i)
            {
                Assert.Equal(i, actor.TestResults.GetCounts(i));
            }
        }

        [Fact]
        public void TestThatSuspendResumes()
        {
            const string paused = "paused#";
            const string exceptional = "exceptional#";

            var dispatcher = new ExecutorDispatcher(1, 0, 1.0f);
            var mailbox = new ConcurrentQueueMailbox(dispatcher, 1);

            mailbox.SuspendExceptFor(paused, typeof(CountTakerActor));

            mailbox.SuspendExceptFor(exceptional, typeof(CountTakerActor));

            mailbox.Resume(exceptional);

            mailbox.Resume(paused);

            Assert.False(mailbox.IsSuspended);
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            public CountTakerActor(TestResults testResults)
            {
                TestResults = testResults;
            }

            public TestResults TestResults { get; }

            public void Take(int count) => TestResults.AddCount(count);
        }

        private class TestResults
        {
            private readonly AccessSafely _safely;

            public TestResults(int happenings)
            {
                var list = new List<int>();
                _safely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("counts", list.Add)
                    .ReadingWith<int, int>("counts", i => list[i]);
            }

            public void AddCount(int count) => _safely.WriteUsing("counts", count);

            public int GetCounts(int index) => _safely.ReadFrom<int, int>("counts", index);
        }
    }
}
