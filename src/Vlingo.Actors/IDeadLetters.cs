namespace Vlingo.Actors
{
    public interface IDeadLetters : IStoppable
    {
        void FailedDelivery(DeadLetter deadLetter);
        void RegisterListener(IDeadLettersListener listener);
    }
}