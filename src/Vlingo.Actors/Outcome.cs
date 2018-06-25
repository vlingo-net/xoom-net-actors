namespace Vlingo.Actors
{
    public abstract class Outcome<TOutcome>
    {
        protected Outcome(TOutcome value)
        {
            Value = value;
        }

        public TOutcome Value { get; }
    }
}