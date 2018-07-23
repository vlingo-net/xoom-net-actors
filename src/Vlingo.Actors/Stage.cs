using System;
using Vlingo.Actors.TestKit;

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

        public T ActorFor<T>(Definition definition)
        {
            throw new NotImplementedException();
        }

        public Scheduler Scheduler { get; }

        public ActorProtocolActor<object>[] ActorProtocolFor<T>(
          Definition definition,
          Actor parent,
          Address maybeAddress,
          IMailbox maybeMailbox,
          ISupervisor maybeSupervisor,
          ILogger logger)
        {
            throw new NotImplementedException();
        }
        
        ActorProtocolActor<T> actorProtocolFor<T>(
            Definition definition,
            Actor parent,
            Address maybeAddress,
            IMailbox maybeMailbox,
            ISupervisor maybeSupervisor,
            ILogger logger) 
        {

            try {
                Actor actor = CreateRawActor(definition, parent, maybeAddress, maybeMailbox, maybeSupervisor, logger);
                T protocolActor = ActorProxyFor<T>(actor, actor.LifeCycle.Environment.Mailbox);
                return new ActorProtocolActor<T>(actor, protocolActor);
            } catch (Exception e) {
                e.printStackTrace();
                world.defaultLogger().log("vlingo/actors: FAILED: " + e.getMessage(), e);
                return null;
            }
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

        internal TestActor<T> TestActorFor<T>(Definition definition)
        {
            throw new NotImplementedException();
        }

        internal Protocols TestActorFor(Definition definition, Type[] protocols)
        {
            throw new NotImplementedException();
        }
    }
}