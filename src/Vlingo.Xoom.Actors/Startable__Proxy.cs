// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
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
            Action<IStartable> consumer = x => x.Start();
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
