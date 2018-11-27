// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class Cancellable__Proxy : ICancellable
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Cancellable__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }
        public bool Cancel()
        {
            if (!actor.IsStopped)
            {
                Action<ICancellable> consumer = actor => actor.Cancel();
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, "Cancel()");
                }
                else
                {
                    mailbox.Send(new LocalMessage<ICancellable>(actor, consumer, "Cancel()"));
                }
                
                return true;
            }

            actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Cancel()"));
            return false;
        }
    }
}
