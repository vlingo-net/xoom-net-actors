namespace Vlingo.Actors
{
    public interface IOutcomeAware<TO, TR>
    {
        void FailureOutcome(Outcome<TO> outcome, TR reference);
        void SuccessfulOutcome(Outcome<TO> outcome, TR reference);
    }
}