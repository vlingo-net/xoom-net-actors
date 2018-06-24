namespace Vlingo.Actors
{
    public interface IOutcomeInterest<TOutcome>
    {
        void FailureOutcome(Outcome<TOutcome> outcome);
        void SuccessfulOutcome(Outcome<TOutcome> outcome);
    }
}