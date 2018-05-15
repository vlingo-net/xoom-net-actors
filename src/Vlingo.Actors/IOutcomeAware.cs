namespace Vlingo.Actors
{
    public interface IOutcomeAware<TOutcome, TRef>
    {
        void FailureOutcome(Outcome<TOutcome> outcome, TRef reference);
        void SuccessfulOutcome(Outcome<TOutcome> outcome, TRef reference);
    }
}