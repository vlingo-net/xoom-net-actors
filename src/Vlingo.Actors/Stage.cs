// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Stage : IStoppable
    {
        private readonly IDictionary<string, ISupervisor> commonSupervisors;
        private readonly Directory directory;
        private readonly Scheduler scheduler;
        private AtomicBoolean stopped;

        public Stage(World world, string name)
        {
            World = world;
            Name = name;
            directory = new Directory();
            commonSupervisors = new Dictionary<string, ISupervisor>();
            scheduler = new Scheduler();
            stopped = new AtomicBoolean(false);
        }

        public T ActorAs<T>(Actor actor)
            => ActorProxyFor<T>(actor, actor.LifeCycle.Environment.Mailbox);

        public T ActorFor<T>(Definition definition)
            => ActorFor<T>(
                definition,
                definition.ParentOr(World.DefaultParent),
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

        public Protocols ActorFor(Definition definition, Type[] protocols)
            => new Protocols(ActorFor(
                definition,
                protocols,
                definition.ParentOr(World.DefaultParent),
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger)));

        public TestActor<T> TestActorFor<T>(Definition definition)
        {
            var redefinition = Definition.Has(
                definition.Type,
                definition.Parameters(),
                TestMailbox.Name,
                definition.ActorName);

            try
            {
                return ActorFor<T>(
                    redefinition,
                    definition.ParentOr(World.DefaultParent),
                    null,
                    null,
                    definition.Supervisor,
                    definition.LoggerOr(World.DefaultLogger)).ToTestActor();

            }
            catch (Exception e)
            {
                World.DefaultLogger.Log($"vlingo-net/actors: FAILED: {e.Message}", e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public Protocols TestActorFor(Definition definition, Type[] protocols)
        {
            var redefinition = Definition.Has(
                definition.Type,
                definition.Parameters(),
                TestMailbox.Name,
                definition.ActorName);

            var all = ActorFor(
                redefinition,
                protocols,
                definition.ParentOr(World.DefaultParent),
                null,
                null,
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

            return new Protocols(ActorProtocolActor<object>.ToTestActors(all));
        }

        public T ActorProxyFor<T>(Actor actor, IMailbox mailbox)
            => ActorProxy.CreateFor<T>(actor, mailbox);

        public object ActorProxyFor(Type protocol, Actor actor, IMailbox mailbox)
            => ActorProxy.CreateFor(protocol, actor, mailbox);

        public object[] ActorProxyFor(Type[] protocols, Actor actor, IMailbox mailbox)
        {
            var proxies = new object[protocols.Length];

            for (int idx = 0; idx < protocols.Length; ++idx)
            {
                proxies[idx] = ActorProxyFor(protocols[idx], actor, mailbox);
            }

            return proxies;
        }

        public int Count => directory.Count;

        public void Dump()
        {
            var logger = World.DefaultLogger;
            if (logger.IsEnabled)
            {
                logger.Log($"STAGE: {Name}");
                directory.Dump(logger);
            }
        }

        public string Name { get; }

        public void RegisterCommonSupervisor(string fullyQualifiedProtocol, ISupervisor common)
            => commonSupervisors[fullyQualifiedProtocol] = common;

        public Scheduler Scheduler { get; }

        public bool IsStopped => stopped.Get();

        public void Stop()
        {
            if (!stopped.CompareAndSet(false, true))
            {
                return;
            }

            Sweep();

            int retries = 0;
            while (Count > 1 && ++retries < 10)
            {
                try { Thread.Sleep(10); } catch (Exception) { }
            }

            scheduler.Close();
        }

        public World World { get; }

        internal T ActorFor<T>(Definition definition, Actor parent, ISupervisor maybeSupervisor, ILogger logger)
        {
            var actor = ActorFor<T>(definition, parent, null, null, maybeSupervisor, logger);
            return actor.ProtocolActor;
        }

        internal ActorProtocolActor<object>[] ActorFor(Definition definition, Type[] protocols, Actor parent, ISupervisor maybeSupervisor, ILogger logger)
        {
            return ActorFor(definition, protocols, parent, null, null, maybeSupervisor, logger);
        }

        internal ActorProtocolActor<T> ActorFor<T>(
            Definition definition, 
            Actor parent,
            Address maybeAddress,
            IMailbox maybeMailbox,
            ISupervisor maybeSupervisor,
            ILogger logger)
        {
            try
            {
                var actor = CreateRawActor(definition, parent, maybeAddress, maybeMailbox, maybeSupervisor, logger);
                var protocolActor = ActorProxyFor<T>(actor, actor.LifeCycle.Environment.Mailbox);
                return new ActorProtocolActor<T>(actor, protocolActor);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                World.DefaultLogger.Log($"vlingo/actors: FAILED: {e.Message}", e);
                return null;
            }
        }

        internal ActorProtocolActor<object>[] ActorFor(
            Definition definition,
            Type[] protocols,
            Actor parent,
            Address maybeAddress,
            IMailbox maybeMailbox,
            ISupervisor maybeSupervisor,
            ILogger logger)
        {
            try
            {
                var actor = CreateRawActor(definition, parent, maybeAddress, maybeMailbox, maybeSupervisor, logger);
                var protocolActors = ActorProxyFor(protocols, actor, actor.LifeCycle.Environment.Mailbox);
                return ActorProtocolActor<object>.AllOf(actor, protocolActors);
            }
            catch (Exception e)
            {
                World.DefaultLogger.Log($"vlingo/actors: FAILED: {e.Message}", e);
                return null;
            }
        }

        internal ISupervisor CommonSupervisorOr<T>(ISupervisor defaultSupervisor)
            => commonSupervisors[typeof(T).FullName] ?? defaultSupervisor;

        internal void HandleFailureOf<T>(ISupervised supervised)
        {
            supervised.Suspend();
            supervised.Supervisor.Inform(supervised.Error, supervised);
        }

        internal void Stop(Actor actor)
        {
            var removedActor = directory.Remove(actor.Address);

            if(actor == removedActor)
            {
                removedActor.LifeCycle.Stop(actor);
            }
        }

        private Actor CreateRawActor(
          Definition definition,
          Actor parent,
          Address maybeAddress,
          IMailbox maybeMailbox,
          ISupervisor maybeSupervisor,
          ILogger logger)
        {
            if (IsStopped)
            {
                throw new InvalidOperationException("Actor stage has been stopped.");
            }

            var address = maybeAddress ?? Address.From(definition.ActorName);
            if (directory.IsRegistered(address))
            {
                throw new InvalidOperationException("Address already exists: " + address);
            }

            var mailbox = maybeMailbox ?? ActorFactory.ActorMailbox(this, address, definition);

            Actor actor;

            try
            {
                actor = ActorFactory.ActorFor(this, parent, definition, address, mailbox, maybeSupervisor, logger);
            }
            catch(Exception e)
            {
                logger.Log($"Actor instantiation failed because: {e.Message}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw new InvalidOperationException($"Actor instantiation failed because: {e.Message}", e);
            }

            directory.Register(actor.Address, actor);
            actor.LifeCycle.BeforeStart(actor);

            return actor;
        }

        private void Sweep()
        {
            if (World.PrivateRoot != null)
            {
                World.PrivateRoot.Stop();
            }
        }
    }

    internal class ActorProtocolActor<T>
    {
        private readonly Actor actor;

        internal ActorProtocolActor(Actor actor, T protocol)
        {
            this.actor = actor;
            ProtocolActor = protocol;
        }

        internal T ProtocolActor { get; }

        internal static ActorProtocolActor<T>[] AllOf(Actor actor, T[] protocolActors)
        {
            var all = new ActorProtocolActor<T>[protocolActors.Length];
            for (int idx = 0; idx < protocolActors.Length; ++idx)
            {
                all[idx] = new ActorProtocolActor<T>(actor, protocolActors[idx]);
            }
            return all;
        }

        internal static TestActor<T>[] ToTestActors(ActorProtocolActor<T>[] all)
        {
            var testActors = new TestActor<T>[all.Length];
            for (int idx = 0; idx < all.Length; ++idx)
            {
                testActors[idx] = all[idx].ToTestActor();
            }

            return testActors;
        }

        internal TestActor<T> ToTestActor() => new TestActor<T>(actor, ProtocolActor, actor.Address);
    }
}