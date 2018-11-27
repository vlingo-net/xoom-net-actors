// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class DeadLettersListener__Proxy : IDeadLettersListener
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public DeadLettersListener__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void Handle(DeadLetter deadLetter)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLettersListener> consumer = actor => actor.Handle(deadLetter);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, "Handle(DeadLetter)");
                }
                else
                {
                    mailbox.Send(new LocalMessage<IDeadLettersListener>(actor, consumer, "Handle(DeadLetter)"));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Handle(DeadLetter)"));
            }
        }
    }
}
