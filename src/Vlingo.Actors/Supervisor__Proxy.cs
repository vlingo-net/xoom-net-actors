// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
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

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Supervisor__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public ISupervisionStrategy SupervisionStrategy => null!;

        public ISupervisor Supervisor => new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            if (!_actor.IsStopped)
            {
                Action<ISupervisor> consumer = x => x.Inform(error, supervised);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationInform1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ISupervisor>(_actor, consumer, RepresentationInform1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationInform1));
            }
        }
    }
}
