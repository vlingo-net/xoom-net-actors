// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
{
    public class Stoppable__Proxy : IStoppable
    {
        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Stoppable__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void Conclude()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = x => x.Conclude();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "Conclude()");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, "Conclude()"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "Conclude()"));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = x => x.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "Stop()");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, "Stop()"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "Stop()"));
            }
        }
    }
}
