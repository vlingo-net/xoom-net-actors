// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class Supervisor__Proxy : ISupervisor
    {
        private const string RepresentationInform1 = "inform(Throwable, Supervised)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Supervisor__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public ISupervisionStrategy? SupervisionStrategy => null;

        public ISupervisor Supervisor => new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            if (!actor.IsStopped)
            {
                Action<ISupervisor> consumer = x => x.Inform(error, supervised);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, RepresentationInform1);
                }
                else
                {
                    mailbox.Send(new LocalMessage<ISupervisor>(actor, consumer, RepresentationInform1));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationInform1));
            }
        }
    }
}
