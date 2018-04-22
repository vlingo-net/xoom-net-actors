namespace Vlingo.Actors
{
    public abstract class Outcome<TO>
    {
        protected Outcome(TO value) {
            this.Value = value;
        }

        public TO Value { get; }
    }
}