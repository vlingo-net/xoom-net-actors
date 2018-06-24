namespace Vlingo.Actors
{
    public class CompletesHolder : ICompletes<object>
    {
        public CompletesHolder(
            long id,
            ICompletes<object> clientCompletes,
            ICompletesEventually completesEventually)
        {
            Id = id;
            ClientCompletes = clientCompletes;
            CompletesEventually = completesEventually;
        }

        public long Id { get; }
        public ICompletes<object> ClientCompletes { get; }
        public ICompletesEventually CompletesEventually { get; }
        public object Outcome { get; private set; }

        public void With(object outcome)
        {
            Outcome = outcome;
            CompletesEventually.With(this);
        }
    }
}
