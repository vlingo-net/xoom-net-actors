namespace Vlingo.Actors
{
    public class PooledCompletes<T> : ICompletesEventually
    {
        public long Id { get; }

        public ICompletes<T> ClientCompletes { get; }

        public ICompletesEventually CompletesEventually { get; }

        public PooledCompletes(
            long id,
            ICompletes<T> clientCompletes,
            ICompletesEventually completesEventually)
        {
            Id = id;
            ClientCompletes = clientCompletes;
            CompletesEventually = completesEventually;
        }

        public object Outcome { get; private set; }

        public void With(object outcome)
        {
            Outcome = outcome;
        }

        public bool IsStopped => CompletesEventually.IsStopped;

        public void Stop()
        {
        }
    }
}
