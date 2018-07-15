// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class OutcomeAware__Proxy<TOutcome, TRef> : IOutcomeAware<TOutcome, TRef>
    {
        private const string FailureOutcomeRepesentation1 = "failureOutcome(Outcome<O>, R)";
        private const string SuccessfulOutcomeRepesentation2 = "successfulOutcome(Outcome<O>, R)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public OutcomeAware__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void FailureOutcome(Outcome<TOutcome> outcome, TRef reference)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeAware<TOutcome, TRef>> consumer = actor => actor.FailureOutcome(outcome, reference);
                mailbox.Send(new LocalMessage<IOutcomeAware<TOutcome, TRef>>(actor, consumer, FailureOutcomeRepesentation1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, FailureOutcomeRepesentation1));
            }
        }

        public void SuccessfulOutcome(Outcome<TOutcome> outcome, TRef reference)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeAware<TOutcome, TRef>> consumer = actor => actor.SuccessfulOutcome(outcome, reference);
                mailbox.Send(new LocalMessage<IOutcomeAware<TOutcome, TRef>>(actor, consumer, SuccessfulOutcomeRepesentation2));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, SuccessfulOutcomeRepesentation2));
            }
        }
    }
}
