namespace Vlingo.Actors
{
    public class PooledCompletes : ICompletesEventually
    {
        public long Id { get; }

        public ICompletes<object> ClientCompletes { get; }

        public ICompletesEventually CompletesEventually { get; }

        public PooledCompletes(
            long id,
            ICompletes<object> clientCompletes,
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
