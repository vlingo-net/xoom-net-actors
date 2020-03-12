// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Common;
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
            var dispatcher = new ManyToOneConcurrentArrayQueueDispatcher(MailboxSize, 2, 4, 10);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor();
            
            actor.Until = Until(MailboxSize);
            for (var i = 1; i <= MailboxSize; ++i)
            {
                var countParam = i;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }

            actor.Until.Completes();
            dispatcher.Close();

            const int neverReviewed = MailboxSize * 2;
            for (var count = MailboxSize + 1; count <= neverReviewed; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }
            
            Until(0).Completes();
            
            Assert.Equal(MailboxSize, actor.Highest.Get());
        }

        [Fact]
        public void TestBasicDispatch()
        {
            var dispatcher = new ManyToOneConcurrentArrayQueueDispatcher(MailboxSize, 2, 4, 10);
            dispatcher.Start();
            var mailbox = dispatcher.Mailbox;
            var actor = new CountTakerActor();
            actor.Until = Until(MailboxSize);

            for (var count = 1; count <= MailboxSize; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
            }

            actor.Until.Completes();

            Assert.Equal(MailboxSize, actor.Highest.Get());
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            public AtomicInteger Highest = new AtomicInteger(0);
            public TestUntil Until = TestUntil.Happenings(0);

            public void Take(int count)
            {
                if(count > Highest.Get())
                {
                    Highest.Set(count);
                }
                Until.Happened();
            }
        }
    }
}
