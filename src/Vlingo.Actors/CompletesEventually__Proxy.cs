// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class CompletesEventually__Proxy : ICompletesEventually
    {
        private const string RepresentationConclude = "Conclude()";
        private const string RepresentationStop = "Stop()";
        private const string RepresentationWith = "With(object)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public CompletesEventually__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public IAddress Address => _actor.Address;

        public bool IsStopped => _actor.IsStopped;

        public void Conclude()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = x => x.Conclude();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationConclude);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, RepresentationConclude));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationConclude));
            }
        }

        public void Stop()
        {
            if (!_actor.IsStopped)
            {
                Action<IStoppable> consumer = x => x.Stop();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationStop);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IStoppable>(_actor, consumer, RepresentationStop));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationStop));
            }
        }

        public void With(object? outcome)
        {
            if (!_actor.IsStopped)
            {
                Action<ICompletesEventually> consumer = x => x.With(outcome);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, RepresentationWith);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ICompletesEventually>(_actor, consumer, RepresentationWith));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, RepresentationWith));
            }
        }
    }
}
