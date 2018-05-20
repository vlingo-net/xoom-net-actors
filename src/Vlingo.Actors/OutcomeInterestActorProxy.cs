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
