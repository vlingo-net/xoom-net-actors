namespace Vlingo.Actors
{
    public sealed class DeadLetter
    {
        private readonly Actor actor;
        private readonly string representation;

        public DeadLetter(Actor actor, string representation)
        {
            this.actor = actor;
            this.representation = representation;
        }

        public override string ToString() => $"DeadLetter[{actor}.{representation}]";
    }
}
