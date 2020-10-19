// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public DeadLettersListener__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public void Handle(DeadLetter deadLetter)
        {
            if (!_actor.IsStopped)
            {
                Action<IDeadLettersListener> consumer = x => x.Handle(deadLetter);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "Handle(DeadLetter)");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDeadLettersListener>(_actor, consumer, "Handle(DeadLetter)"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "Handle(DeadLetter)"));
            }
        }
    }
}
