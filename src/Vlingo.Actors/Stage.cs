namespace Vlingo.Actors
{
    public class Stage
    {
        public World World { get; set; }

        public T ActorProxyFor<T>(T protocol, Actor actor, Mailbox mailbox)
        {
            throw new System.NotImplementedException();
        }
    }
}