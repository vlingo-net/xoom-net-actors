// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class OutcomeInterest__Proxy<TOutcome> : IOutcomeInterest<TOutcome>
    {
        private const string FailureOutcomeRepesentation1 = "failureOutcome(Outcome<O>)";
        private const string SuccessfulOutcomeRepesentation2 = "successfulOutcome(Outcome<O>)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public OutcomeInterest__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void FailureOutcome(Outcome<TOutcome> outcome)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeInterest<TOutcome>> consumer = actor => actor.FailureOutcome(outcome);
                mailbox.Send(new LocalMessage<IOutcomeInterest<TOutcome>>(actor, consumer, FailureOutcomeRepesentation1));
                // TODO: please confirm the implementation above. it differs from java version
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, FailureOutcomeRepesentation1));
            }
        }

        public void SuccessfulOutcome(Outcome<TOutcome> outcome)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeInterest<TOutcome>> consumer = actor => actor.SuccessfulOutcome(outcome);
                mailbox.Send(new LocalMessage<IOutcomeInterest<TOutcome>>(actor, consumer, SuccessfulOutcomeRepesentation2));
                // TODO: please confirm the implementation above. it differs from java version
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, SuccessfulOutcomeRepesentation2));
            }
        }
    }
}
