// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class Stoppable__Proxy : IStoppable
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Stoppable__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsStopped => actor.IsStopped;

        public void Stop()
        {
            if (!actor.IsStopped)
            {
                Action<IStoppable> consumer = actor => actor.Stop();
                mailbox.Send(new LocalMessage<IStoppable>(actor, consumer, "Stop()"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Stop()"));
            }
        }
    }
}
