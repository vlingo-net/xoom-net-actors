// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
namespace Vlingo.Actors
{
    class OutcomeInterestActorProxy<TOutcome, TRef> : IOutcomeInterest<TOutcome>
    {
        private readonly IOutcomeAware<TOutcome, TRef> outcomeAware;
        private readonly TRef reference;

        public OutcomeInterestActorProxy(
            IOutcomeAware<TOutcome, TRef> outcomeAware,
            TRef reference)
        {
            this.outcomeAware = outcomeAware;
            this.reference = reference;
        }

        public void FailureOutcome(Outcome<TOutcome> outcome)
        {
            outcomeAware.FailureOutcome(outcome, reference);
        }

        public void SuccessfulOutcome(Outcome<TOutcome> outcome)
        {
            outcomeAware.SuccessfulOutcome(outcome, reference);
        }
    }
}
