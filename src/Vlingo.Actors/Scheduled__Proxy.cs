// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Scheduled__Proxy<T> : IScheduled<T>
    {
        private const string RepresentationIntervalSignal1 = "IntervalSignal(Vlingo.Common.IScheduled<T>, T)";
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Scheduled__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void IntervalSignal(IScheduled<T> scheduled, T data)
        {
            if (!actor.IsStopped)
            {
                Action<IScheduled<T>> consumer = x => x.IntervalSignal(scheduled, data);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, RepresentationIntervalSignal1);
                }
                else
                {
                    mailbox.Send(new LocalMessage<IScheduled<T>>(actor, consumer, RepresentationIntervalSignal1));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationIntervalSignal1));
            }
        }
    }
}
