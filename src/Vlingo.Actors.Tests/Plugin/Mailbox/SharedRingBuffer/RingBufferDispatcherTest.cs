// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferDispatcherTest : ActorsTest
    {
        [Fact]
        public void TestClose()
        {
            var testResults = new TestResults();
            const int mailboxSize = 64;
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, 4);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);
            testResults.Until = Until(mailboxSize);

            for(var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            testResults.Until.Completes();
            dispatcher.Close();

            const int neverReceived = mailboxSize * 2;
            for(var count = mailboxSize + 1; count <= neverReceived; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            Until(0).Completes();

            Assert.Equal(mailboxSize, testResults.Highest.Get());
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var testResults = new TestResults();
            const int mailboxSize = 64;
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, 4);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);
            testResults.Until = Until(mailboxSize);

            for (var count = 1; count <= mailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            testResults.Until.Completes();

            Assert.Equal(mailboxSize, testResults.Highest.Get());
        }

        [Fact]
        public void TestOverflowDispatch()
        {
            var testResults = new TestResults();
            const int mailboxSize = 64;
            const int overflowSize = mailboxSize * 2;
            var dispatcher = new RingBufferDispatcher(mailboxSize, 2, 4);
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor(testResults);
            testResults.Until = Until(overflowSize);

            for (var count = 1; count <= overflowSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                mailbox.Send(actor, consumer, null, "Take(int)");
            }

            dispatcher.Start();
            testResults.Until.Completes();

            Assert.Equal(overflowSize, testResults.Highest.Get());
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults testResults;

            public CountTakerActor(TestResults testResults)
            {
                this.testResults = testResults;
            }
            public void Take(int count)
            {
                if(count > testResults.Highest.Get())
                {
                    testResults.Highest.Set(count);
                }
                testResults.Until.Happened();
            }
        }
        private class TestResults
        {
            public AtomicInteger Highest = new AtomicInteger(0);
            public TestUntil Until = TestUntil.Happenings(0);
        }
    }
}
