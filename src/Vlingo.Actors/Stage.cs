using System;

namespace Vlingo.Actors
{
    public class Stage
    {
        public World World { get; set; }

        public T ActorProxyFor<T>(Actor actor, IMailbox mailbox)
        {
            throw new System.NotImplementedException();
        }

        public T ActorFor<T>(Definition definition, Actor actor, ISupervisor supervisor, Logger logger)
        {
            throw new NotImplementedException();
        }

        internal T ActorAs<T>(Actor parent)
        {
            throw new NotImplementedException();
        }
    }
}