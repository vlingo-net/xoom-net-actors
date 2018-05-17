namespace Vlingo.Actors
{
    public class FailureOutcome<T> : Outcome<T>
    {
        public FailureOutcome(T value) : base(value)
        {

        }
    }
}
