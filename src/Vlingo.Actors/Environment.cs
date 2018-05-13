namespace Vlingo.Actors
{
    public class Environment
    {
        public Stage Stage { get; set; }
        public Logger Logger { get; set; }
        public bool IsSecured { get; set; }
        public Actor Parent { get; set; }
        public Mailbox Mailbox { get; set; }
        public Stowage Stowage { get; set; }
    }
}
