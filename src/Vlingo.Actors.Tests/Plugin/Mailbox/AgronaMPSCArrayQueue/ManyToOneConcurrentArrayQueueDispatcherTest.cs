// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Xoom.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueDispatcherTest : ActorsTest
    {
        private const int MailboxSize = 64;

        [Fact]
        public void TestClose()
        {
            var testResults = new TestResults(MailboxSize);
            var dispatcher = new ManyToOneConcurrentArrayQueueDispatcher(MailboxSize, 2, false, 4, 10);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);
            
            for (var i = 1; i <= MailboxSize; ++i)
            {
                var countParam = i;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }

            Assert.Equal(MailboxSize, testResults.GetHighest());
            dispatcher.Close();

            const int neverReceived = MailboxSize * 2;
            for (var count = MailboxSize + 1; count <= neverReceived; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }
            
            Assert.Equal(MailboxSize, testResults.GetHighest());
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var testResult = new TestResults(MailboxSize);
            var dispatcher = new ManyToOneConcurrentArrayQueueDispatcher(MailboxSize, 2, false, 4, 10);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResult);

            for (var count = 1; count <= MailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }

            Assert.Equal(MailboxSize, testResult.GetHighest());
        }
        
        [Fact]
        public void TestNotifyOnSendDispatch()
        {
            var mailboxSize = 64;
            var testResults = new TestResults(mailboxSize);

            var dispatcher = new ManyToOneConcurrentArrayQueueDispatcher(mailboxSize, 1000, true, 4, 10);

            dispatcher.Start();

            var mailbox = dispatcher.Mailbox;

            var actor = new CountTakerActor(testResults);

            for (var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var localMessage = new LocalMessage<ICountTaker>(actor, consumer, "take(int)");

                // notify if in back off
                mailbox.Send(localMessage);

                // every third message give time for dispatcher to back off
                if (count % 3 == 0)
                {
                    Thread.Sleep(50);
                }
            }

            Assert.Equal(mailboxSize, testResults.GetHighest());
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults _testResults;
            private readonly ICountTaker _self;

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
        
        private class TestResults
        {
            private readonly AccessSafely _accessSafely;

            public TestResults(int happenings)
            {
                var highest = new AtomicInteger(0);
                _accessSafely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int>("highest", highest.Set)
                    .ReadingWith("highest", highest.Get)
                    .ReadingWith<int, bool>("isHighest", count => count > highest.Get());
            }

            public void SetHighest(int value) => _accessSafely.WriteUsing("highest", value);

            public int GetHighest() => _accessSafely.ReadFrom<int>("highest");

            public bool IsHighest(int value) => _accessSafely.ReadFromNow<int, bool>("isHighest", value);
        }
    }
}
