// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class DeadLetters__Proxy : IDeadLetters
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public DeadLetters__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsStopped => actor.IsStopped;

        public void Stop()
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.Stop();
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "Stop()"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Stop()"));
            }
        }

        public void FailedDelivery(DeadLetter deadLetter)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.FailedDelivery(deadLetter);
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "FailedDelivery(DeadLetter)"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "FailedDelivery(DeadLetter)"));
            }
        }

        public void RegisterListener(IDeadLettersListener listener)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.RegisterListener(listener);
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "RegisterListener(DeadLettersListener)"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "RegisterListener(DeadLettersListener)"));
            }
        }
    }
}
