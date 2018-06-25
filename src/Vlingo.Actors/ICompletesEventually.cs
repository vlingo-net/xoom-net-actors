namespace Vlingo.Actors
{
    public interface ICompletesEventually : IStoppable
    {
        void With(object outcome);
    }

    public abstract class CompletesEventually : ICompletesEventually
    {
        public bool IsStopped => false;

        public void Stop()
        {
        }

        public abstract void With(object outcome);
    }
}
