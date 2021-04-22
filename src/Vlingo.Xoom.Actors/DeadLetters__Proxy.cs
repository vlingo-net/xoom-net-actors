// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
{
    public class DeadLetters__Proxy : IDeadLetters
    {
        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public DeadLetters__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsStopped => _actor.IsStopped;

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IDeadLetters> consumer = x => x.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "Stop()");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDeadLetters>(_actor, consumer, "Stop()"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "Stop()"));
            }
        }

        public void FailedDelivery(DeadLetter deadLetter)
        {
            if (!_actor.IsStopped)
            {
                Action<IDeadLetters> consumer = x => x.FailedDelivery(deadLetter);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "FailedDelivery(DeadLetter)");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDeadLetters>(_actor, consumer, "FailedDelivery(DeadLetter)"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "FailedDelivery(DeadLetter)"));
            }
        }

        public void RegisterListener(IDeadLettersListener listener)
        {
            if (!_actor.IsStopped)
            {
                Action<IDeadLetters> consumer = x => x.RegisterListener(listener);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "RegisterListener(DeadLettersListener)");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDeadLetters>(_actor, consumer, "RegisterListener(DeadLettersListener)"));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "RegisterListener(DeadLettersListener)"));
            }
        }

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
    }
}
