// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferDispatcherTest : ActorsTest
    {
        [Fact]
        public void TestClose()
        {
            const int mailboxSize = 64;
            var testResults = new TestResults(mailboxSize);
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, false, 4);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);

            for(var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            
            Assert.Equal(mailboxSize, testResults.GetHighest());
            dispatcher.Close();

            const int neverReceived = mailboxSize * 2;
            for(var count = mailboxSize + 1; count <= neverReceived; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            Until(0).Completes();

            Assert.Equal(mailboxSize, testResults.GetHighest());
        }

        [Fact]
        public void TestBasicDispatch()
        {
            const int mailboxSize = 64;
            var testResults = new TestResults(mailboxSize);
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, false, 4);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);

            for (var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            Assert.Equal(mailboxSize, testResults.GetHighest());
        }

        [Fact]
        public void TestOverflowDispatch()
        {
            const int mailboxSize = 64;
            const int overflowSize = mailboxSize * 2;
            var testResults = new TestResults(mailboxSize);
            
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, false, 4);
            
            var mailbox = dispatcher.Mailbox;
            
            var actor = new CountTakerActor(testResults);

            for (var count = 1; count <= overflowSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            dispatcher.Start();

            Assert.Equal(overflowSize, testResults.GetHighest());
        }

        [Fact]
        public void TestNotifyOnSendDispatch()
        {
            var mailboxSize = 64;
            var testResults = new TestResults(mailboxSize);

            var dispatcher = new RingBufferDispatcher(mailboxSize, 1000, true, 4);

            dispatcher.Start();

            var mailbox = dispatcher.Mailbox;

            var actor = new CountTakerActor(testResults);

            for (var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);

                // notify if in back off
                mailbox.Send(actor, consumer, null, "take(int)");

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
