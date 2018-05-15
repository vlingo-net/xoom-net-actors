using System;

namespace Vlingo.Actors
{
    class OutcomeInterestActorProxy<TOutcome, TRef> : IOutcomeInterest<TOutcome>
    {
        public OutcomeInterestActorProxy(
            IOutcomeAware<TOutcome, TRef> outcomeAware,
            TRef reference)
        {

        }

        public void FailureOutcome(Outcome<TOutcome> outcome)
        {
            throw new NotImplementedException();
        }

        public void SuccessfulOutcome(Outcome<TOutcome> outcome)
        {
            throw new NotImplementedException();
        }
    }
}
