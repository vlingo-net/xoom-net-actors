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

        public T ActorFor<T>(Definition definition, Actor actor, ISupervisor supervisor, ILogger logger)
        {
            throw new NotImplementedException();
        }

        public Scheduler Scheduler { get; }

        public ActorProtocolActor<object>[] ActorFor<T>(
          Definition definition,
          Actor parent,
          Address maybeAddress,
          IMailbox maybeMailbox,
          ISupervisor maybeSupervisor,
          ILogger logger)
        {
            throw new NotImplementedException();
        }

        internal T ActorAs<T>(Actor parent)
        {
            throw new NotImplementedException();
        }

        internal void HandleFailureOf<T>(ISupervised supervised)
        {
            throw new NotImplementedException();
        }

        internal ISupervisor CommonSupervisorOr(ISupervisor defaultSupervisor)
        {
            throw new NotImplementedException();
        }

        public class ActorProtocolActor<T> { }
    }
}