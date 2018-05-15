namespace Vlingo.Actors
{
    public interface IDeadLetters
    {
        void FailedDelivery(DeadLetter deadLetter);
        void RegisterListener(IDeadLettersListener listener);
    }
}