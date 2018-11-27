// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
        private const string RepresentationStop = "Stop()";
        private const string RepresentationWith = "With(object)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public CompletesEventually__Proxy(Actor actor, IMailbox mailbox)
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
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, RepresentationStop);
                }
                else
                {
                    mailbox.Send(new LocalMessage<IStoppable>(actor, consumer, RepresentationStop));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationStop));
            }
        }

        public void With(object outcome)
        {
            if (!actor.IsStopped)
            {
                Action<ICompletesEventually> consumer = actor => actor.With(outcome);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, RepresentationWith);
                }
                else
                {
                    mailbox.Send(new LocalMessage<ICompletesEventually>(actor, consumer, RepresentationWith));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationWith));
            }
        }
    }
}
