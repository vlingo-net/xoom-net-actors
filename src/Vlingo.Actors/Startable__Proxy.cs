// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class Startable__Proxy : IStartable
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Startable__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void Start()
        {
            Action<IStartable> consumer = actor => actor.Start();
            if (mailbox.IsPreallocated)
            {
                mailbox.Send(actor, consumer, null, "Start()");
            }
            else
            {
                mailbox.Send(new LocalMessage<IStartable>(actor, consumer, "Start()"));
            }
        }
    }
}
