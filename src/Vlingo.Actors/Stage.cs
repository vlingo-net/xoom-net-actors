// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vlingo.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Stage : IStoppable
    {
        private readonly IDictionary<Type, ISupervisor> commonSupervisors;
        private readonly Directory directory;
        private IDirectoryScanner? directoryScanner;
        private readonly Scheduler scheduler;
        private AtomicBoolean stopped;

        /// <summary>
        /// Initializes the new <c>Stage</c> of the world and with name. (INTERNAL ONLY)
        /// </summary>
        /// <param name="world">The <c>World</c> parent of this <c>Stage</c>.</param>
        /// <param name="name">The <c>string</c> name of this <c>Stage</c>.</param>
        internal Stage(World world, string name)
        {
            World = world;
            Name = name;
            directory = new Directory(world.AddressFactory.None());
            commonSupervisors = new Dictionary<Type, ISupervisor>();
            scheduler = new Scheduler();
            stopped = new AtomicBoolean(false);
        }

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol type as the means to message the backing <c>Actor</c>.
        /// </summary>
        /// <typeparam name="T">The protocol.</typeparam>
        /// <param name="actor">The <c>Actor</c> that implements the <typeparamref name="T"/> protocol</param>
        /// <returns>The <paramref name="actor"/> as <typeparamref name="T"/></returns>
        public T ActorAs<T>(Actor actor)
            => ActorProxyFor<T>(actor, actor.LifeCycle.Environment.Mailbox);

        /// <summary>
        /// Answers the <code>T</code> protocol of the newly created <code>Actor</code> that implements the <code>protocol</code>.
        /// </summary>
        /// <typeparam name="T">The protocol type</typeparam>
        /// <param name="type">The type of <code>Actor</code> to create</param>
        /// <param name="parameters">Constructor parameters for the <code>Actor</code></param>
        /// <returns></returns>
        public T ActorFor<T>(Type type, params object[] parameters)
            => ActorFor<T>(Definition.Has(type, parameters.ToList()));

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol of the newly created <c>Actor</c> that implements the protocol.
        /// </summary>
        /// <typeparam name="T">The protocol type.</typeparam>
        /// <param name="definition">The <c>Definition</c> used to initialize the newly created <c>Actor</c>.</param>
        /// <returns>The <c>Actor</c> as <typeparamref name="T"/>.</returns>
        public T ActorFor<T>(Definition definition)
            => ActorFor<T>(
                definition,
                definition.ParentOr(World.DefaultParent),
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol of the newly created <c>Actor</c> that implements the protocol and
        /// that will be assigned the specific <paramref name="address"/> and <paramref name="address"/>.
        /// </summary>
        /// <typeparam name="T">The protocol</typeparam>
        /// <param name="definition">The <c>Definition</c> used to initialize the newly created <c>Actor</c>.</param>
        /// <param name="address">The <c>IAddress</c> to assign to the newly created <c>Actor</c>.</param>
        /// <param name="logger">The <c>ILogger</c> to assign to the newly created <c>Actor</c>.</param>
        /// <returns>The <c>Actor</c> as <typeparamref name="T"/>.</returns>
        public T ActorFor<T>(Definition definition, IAddress address, ILogger logger)
        {
            var actorAddress = AllocateAddress(definition, address);
            var actorMailbox = AllocateMailbox(definition, actorAddress, null);

            var actor = ActorProtocolFor<T>(
                definition,
                definition.ParentOr(World.DefaultParent),
                actorAddress,
                actorMailbox,
                definition.Supervisor,
                logger);

            return actor!.ProtocolActor;
        }

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol of the newly created <c>Actor</c> that implements the protocol and
        /// that will be assigned the specific <paramref name="logger"/>.
        /// </summary>
        /// <typeparam name="T">The protocol.</typeparam>
        /// <param name="definition">The <c>Definition</c> used to initialize the newly created <c>Actor</c>.</param>
        /// <param name="logger">The <c>ILogger</c> to assign to the newly created <c>Actor</c>.</param>
        /// <returns>The <c>Actor</c> as <typeparamref name="T"/>.</returns>
        public T ActorFor<T>(Definition definition, ILogger logger)
            => ActorFor<T>(
                definition,
                definition.ParentOr(World.DefaultParent),
                definition.Supervisor,
                logger);

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol of the newly created <c>Actor</c> that implements the protocol and
        /// that will be assigned the specific <paramref name="address"/>.
        /// </summary>
        /// <typeparam name="T">The protocol.</typeparam>
        /// <param name="definition">The <c>Definition</c> used to initialize the newly created <c>Actor</c>.</param>
        /// <param name="address">The <c>IAddress</c> to assign to the newly created <c>Actor</c>.</param>
        /// <returns>The <c>Actor</c> as <typeparamref name="T"/>.</returns>
        public T ActorFor<T>(Definition definition, IAddress address)
        {
            var actorAddress = AllocateAddress(definition, address);
            var actorMailbox = AllocateMailbox(definition, actorAddress, null);

            var actor = ActorProtocolFor<T>(
                definition,
                definition.ParentOr(World.DefaultParent),
                actorAddress,
                actorMailbox,
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

            return actor!.ProtocolActor;
        }

        /// <summary>
        /// Answers a <code>Protocols</code> that provides one or more supported <paramref name="protocols"/> for the
        /// newly created <code>Actor</code> according to <paramref name="definition"/>.
        /// </summary>
        /// <param name="protocols">Array of protocols that the <code>Actor</code> supports.</param>
        /// <param name="definition">The definition providing parameters to the <code>Actor</code>.</param>
        /// <param name="parent">The actor that is this actor's parent.</param>
        /// <param name="maybeSupervisor">The possible supervisor of this actor.</param>
        /// <param name="logger">The logger of this actor.</param>
        /// <returns></returns>
        public Protocols ActorFor(Type[] protocols, Definition definition, Actor parent, ISupervisor? maybeSupervisor, ILogger logger)
        {
            var all = ActorProtocolFor(protocols, definition, parent, maybeSupervisor, logger);
            return new Protocols(ActorProtocolActor<object>.ToActors(all));
        }

        /// <summary>
        /// Answers a <c>Protocols</c> that provides one or more supported <paramref name="protocols"/> for the
        /// newly created <c>Actor</c> according to <paramref name="definition"/>.
        /// </summary>
        /// <param name="protocols">The array of protocol that the <c>Actor</c> supports.</param>
        /// <param name="definition">The <c>Definition</c> providing parameters to the <c>Actor</c>.</param>
        /// <returns>A <see cref="Protocols"/> instance.</returns>
        public Protocols ActorFor(Type[] protocols, Definition definition)
        {
            var all = ActorProtocolFor(
                protocols,
                definition,
                definition.ParentOr(World.DefaultParent),
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

            return new Protocols(ActorProtocolActor<object>.ToActors(all));
        }

        /// <summary>
        /// Answers a <c>Protocols</c> that provides one or more supported <paramref name="protocols"/> for the
        /// newly created <c>Actor</c> with the provided <paramref name="parameters"/>.
        /// </summary>
        /// <param name="protocols">The array of protocol that the <c>Actor</c> supports.</param>
        /// <param name="type">The type of the <c>Actor</c> to create.</param>
        /// <param name="parameters">The constructor parameters.</param>
        /// <returns>A <see cref="Protocols"/> instance.</returns>
        public Protocols ActorFor(Type[] protocols, Type type, params object[] parameters)
            => ActorFor(protocols, Definition.Has(type, parameters.ToList()));

        /// <summary>
        /// Answers the <c>ICompletes&lt;T&gt;</c> that will eventually complete with the <typeparamref name="T"/> protocol
        /// of the backing <c>Actor</c> of the given <paramref name="address"/>, or <c>null</c> if not found.   
        /// </summary>
        /// <typeparam name="T">The protocol supported by the backing <c>Actor</c>.</typeparam>
        /// <param name="address">The <c>IAddress</c> of the <c>Actor</c> to find.</param>
        /// <returns>ICompletes&lt;T&gt; of the backing actor found by the address. <c>null</c> if not found.</returns>
        public ICompletes<T> ActorOf<T>(IAddress address) => directoryScanner!.ActorOf<T>(address).AndThen(proxy => proxy);

        /// <summary>
        /// Answer the protocol reference of the actor with <paramref name="address"/> as a non-empty
        /// <code>ICompletes&lt;Optional&lt;T&gt;&gt;</code> eventual outcome, or an empty <code>ICompletes&lt;Optional&lt;T&gt;&gt;</code>
        /// if not found.
        /// </summary>
        /// <typeparam name="T">The protocol that the actor must support.</typeparam>
        /// <param name="address">The address of the actor to find.</param>
        /// <returns></returns>
        public ICompletes<Optional<T>> MaybeActorOf<T>(IAddress address)
            => directoryScanner!.MaybeActorOf<T>(address).AndThen(proxy => proxy);


        /// <summary>
        /// Answers the <c>TestActor&lt;T&gt;</c>, <typeparamref name="T"/> being the protocol, of the new created <c>Actor</c> that implements the protocol.
        /// The <c>TestActor&lt;T&gt;</c> is specifically used for test scenarios and provides runtime access to the internal
        /// <c>Actor</c> instance. Test-based <c>Actor</c> instances are backed by the synchronous <c>TestMailbox</c>.
        /// </summary>
        /// <typeparam name="T">The protocol type.</typeparam>
        /// <param name="definition">the <c>Definition</c> used to initialize the newly created <c>Actor</c>.</param>
        /// <returns></returns>
        public TestActor<T>? TestActorFor<T>(Definition definition)
        {
            var redefinition = Definition.Has(
                definition.Type,
                definition.Parameters(),
                TestMailbox.Name,
                definition.ActorName);

            try
            {
                return ActorProtocolFor<T>(
                    redefinition,
                    definition.ParentOr(World.DefaultParent),
                    null,
                    null,
                    definition.Supervisor,
                    definition.LoggerOr(World.DefaultLogger))!.ToTestActor();

            }
            catch (Exception e)
            {
                World.DefaultLogger.Error($"vlingo-net/actors: FAILED: {e.Message}", e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            return null;
        }

        public TestActor<T>? TestActorFor<T>(Type actorType, params object[] parameters)
            => TestActorFor<T>(Definition.Has(actorType, parameters.ToList()));

        /// <summary>
        /// Answers a <c>Protocols</c> that provides one or more supported <paramref name="protocols"/> for the
        /// newly created <c>Actor</c> according to <paramref name="definition"/>, that can be used for testing.
        /// Test-based <c>Actor</c> instances are backed by the synchronous <c>TestMailbox</c>.
        /// </summary>
        /// <param name="protocols">The array of protocols that the <c>Actor</c> supports.</param>
        /// <param name="definition">The <c>Definition</c> providing parameters to the <c>Actor</c>.</param>
        /// <returns></returns>
        internal Protocols TestActorFor(Type[] protocols, Definition definition)
        {
            var redefinition = Definition.Has(
                definition.Type,
                definition.Parameters(),
                TestMailbox.Name,
                definition.ActorName);

            var all = ActorProtocolFor(
                protocols,
                redefinition,
                definition.ParentOr(World.DefaultParent),
                null,
                null,
                definition.Supervisor,
                definition.LoggerOr(World.DefaultLogger));

            return new Protocols(ActorProtocolActor<object>.ToTestActors(all, protocols));
        }

        /// <summary>
        /// Gets the count of the number of <c>Actor</c> instances contained in this <c>Stage</c>.
        /// </summary>
        public int Count => directory.Count;

        /// <summary>
        /// A debugging tool used to print information about the <c>Actor</c> instances contained in this <c>Stage</c>.
        /// </summary>
        public void Dump()
        {
            var logger = World.DefaultLogger;
            if (logger.IsEnabled)
            {
                logger.Debug($"STAGE: {Name}");
                directory.Dump(logger);
            }
        }

        /// <summary>
        /// Gets the name of this <c>Stage</c>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Registers with this <c>Stage</c> the <paramref name="common"/> supervisor for the given <paramref name="protocol"/>.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="common"></param>
        public void RegisterCommonSupervisor(Type? protocol, ISupervisor common)
        {
            if (protocol != null) commonSupervisors[protocol] = common;
        }

        /// <summary>
        /// Gets the <c>Scheduler</c> of this <c>Stage</c>.
        /// </summary>
        public Scheduler Scheduler => scheduler;

        /// <summary>
        /// Gets whether or not this <c>Stage</c> has been stopped or is in the process of stopping.
        /// </summary>
        public bool IsStopped => stopped.Get();

        public void Conclude() => Stop();

        /// <summary>
        /// Initiates the process of stopping this <c>Stage</c>.
        /// </summary>
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

        /// <summary>
        /// Gets the <c>World</c> instance of this <c>Stage</c>.
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol for the newly created <c>Actor</c> instance. (INTERNAL ONLY)
        /// </summary>
        /// <typeparam name="T">The protocol of the <c>Actor</c>.</typeparam>
        /// <param name="definition">The definition of the <c>Actor</c>.</param>
        /// <param name="parent">The <c>Actor</c> parent of this <c>Actor</c>.</param>
        /// <param name="maybeSupervisor">The possible supervisor of this <c>Actor</c>.</param>
        /// <param name="logger">The logger for this <c>Actor</c>.</param>
        /// <returns></returns>
        internal T ActorFor<T>(Definition definition, Actor? parent, ISupervisor? maybeSupervisor, ILogger logger)
        {
            var actor = ActorProtocolFor<T>(definition, parent, null, null, maybeSupervisor, logger);
            return actor!.ProtocolActor;
        }

        /// <summary>
        /// Answers the <c>ActorProtocolActor&lt;object&gt;[]</c> for the newly created Actor instance. (INTERNAL ONLY)
        /// </summary>
        /// <param name="protocols">The protocols of the <c>Actor</c>.</param>
        /// <param name="definition">The <c>Definition</c> of the <c>Actor</c>.</param>
        /// <param name="parent">The <c>Actor</c> parent of this <c>Actor</c>.</param>
        /// <param name="maybeSupervisor">The possible supervisor of this <c>Actor</c>.</param>
        /// <param name="logger">Teh logger for this <c>Actor</c>.</param>
        /// <returns></returns>
        internal ActorProtocolActor<object>[]? ActorProtocolFor(Type[] protocols, Definition definition, Actor? parent, ISupervisor? maybeSupervisor, ILogger logger)
        {
            AssertProtocolCompliance(protocols);
            return ActorProtocolFor(protocols, definition, parent, null, null, maybeSupervisor, logger);
        }

        /// <summary>
        /// Answers the ActorProtocolActor for the newly created Actor instance. (INTERNAL ONLY)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="parent"></param>
        /// <param name="maybeAddress"></param>
        /// <param name="maybeMailbox"></param>
        /// <param name="maybeSupervisor"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal ActorProtocolActor<T>? ActorProtocolFor<T>(
            Definition definition,
            Actor? parent,
            IAddress? maybeAddress,
            IMailbox? maybeMailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            AssertProtocolCompliance(typeof(T));
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
                World.DefaultLogger.Error($"vlingo-net/actors: FAILED: {e.Message}", e);
            }

            return null;
        }

        /// <summary>
        /// Answers the ActorProtocolActor[] for the newly created Actor instance. (INTERNAL ONLY)
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="definition"></param>
        /// <param name="parent"></param>
        /// <param name="maybeAddress"></param>
        /// <param name="maybeMailbox"></param>
        /// <param name="maybeSupervisor"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        internal ActorProtocolActor<object>[]? ActorProtocolFor(
            Type[] protocols,
            Definition definition,
            Actor? parent,
            IAddress? maybeAddress,
            IMailbox? maybeMailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            AssertProtocolCompliance(protocols);
            try
            {
                var actor = CreateRawActor(definition, parent, maybeAddress, maybeMailbox, maybeSupervisor, logger);
                var protocolActors = ActorProxyFor(protocols, actor, actor.LifeCycle.Environment.Mailbox);
                return ActorProtocolActor<object>.AllOf(protocolActors, actor);
            }
            catch (Exception e)
            {
                World.DefaultLogger.Error($"vlingo-net/actors: FAILED: {e.Message}", e);
            }

            return null;
        }

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol proxy for this newly created Actor. (INTERNAL ONLY)
        /// </summary>
        /// <typeparam name="T">The protocol of the Actor</typeparam>
        /// <param name="actor">The Actor instance that backs the proxy protocol</param>
        /// <param name="mailbox">The Mailbox instance of this Actor</param>
        /// <returns></returns>
        internal T ActorProxyFor<T>(Actor actor, IMailbox mailbox)
            => ActorProxy.CreateFor<T>(actor, mailbox);

        /// <summary>
        /// Answers the <paramref name="protocol"/> proxy for this newly created Actor. (INTERNAL ONLY)
        /// </summary>
        /// <param name="protocol">The protocol of the Actor</param>
        /// <param name="actor">The Actor instance that backs the proxy protocol</param>
        /// <param name="mailbox">The Mailbox instance of this Actor</param>
        /// <returns></returns>
        internal object ActorProxyFor(Type protocol, Actor actor, IMailbox mailbox)
            => ActorProxy.CreateFor(protocol, actor, mailbox);

        /// <summary>
        /// Answers the <c>object[]</c> protocol proxies for this newly created Actor. (INTERNAL ONLY)
        /// </summary>
        /// <param name="protocols">The protocols of the Actor</param>
        /// <param name="actor">The Actor instance that backs the proxy protocol</param>
        /// <param name="mailbox">The Mailbox instance of this Actor</param>
        /// <returns></returns>
        internal object[] ActorProxyFor(Type[] protocols, Actor actor, IMailbox mailbox)
        {
            var proxies = new object[protocols.Length];

            for (int idx = 0; idx < protocols.Length; ++idx)
            {
                proxies[idx] = ActorProxyFor(protocols[idx], actor, mailbox);
            }

            return proxies;
        }

        /// <summary>
        /// Answers the common Supervisor for the given protocol or the defaultSupervisor if there is
        /// no registered common Supervisor. (INTERNAL ONLY)
        /// </summary>
        /// <typeparam name="T">The protocol of the supervisor.</typeparam>
        /// <param name="defaultSupervisor">The default Supervisor to be used if there is no registered common Supervisor.</param>
        /// <returns></returns>
        internal ISupervisor CommonSupervisorOr<T>(ISupervisor defaultSupervisor)
        {
            if (commonSupervisors.TryGetValue(typeof(T), out ISupervisor value))
            {
                return value;
            }

            return defaultSupervisor;
        }

        /// <summary>
        /// Answers my Directory instance. (INTERNAL ONLY)
        /// </summary>
        internal Directory Directory => directory;

        /// <summary>
        /// Handles a failure by suspending the Actor and dispatching to the Supervisor. (INTERNAL ONLY)
        /// </summary>
        /// <param name="supervised">The Supervised instance, which is an Actor</param>
        internal void HandleFailureOf(ISupervised supervised)
        {
            supervised.Suspend();
            supervised.Supervisor.Inform(supervised.Error, supervised);
        }

        /// <summary>
        /// Start the directory scan process in search for a given Actor instance. (INTERNAL ONLY)
        /// </summary>
        internal void StartDirectoryScanner()
        {
            directoryScanner = ActorFor<IDirectoryScanner>(
                Definition.Has<DirectoryScannerActor>(
                    Definition.Parameters(directory)));
        }

        /// <summary>
        /// Stop the given Actor and all its children. The Actor instance is first removed from
        /// the Directory of this Stage. (INTERNAL ONLY)
        /// </summary>
        /// <param name="actor">The <c>Actor</c> to stop.</param>
        internal void Stop(Actor actor)
        {
            var removedActor = directory.Remove(actor.Address);

            if (actor == removedActor)
            {
                removedActor.LifeCycle.Stop(actor);
            }
        }

        /// <summary>
        /// Answers an Address for an Actor. If maybeAddress is allocated answer it; otherwise
        /// answer a newly allocated Address. (INTERNAL ONLY)
        /// </summary>
        /// <param name="definition">The Definition of the newly created Actor</param>
        /// <param name="maybeAddress">The possible address</param>
        /// <returns></returns>
        private IAddress AllocateAddress(Definition definition, IAddress maybeAddress)
            => maybeAddress ?? World.AddressFactory.UniqueWith(definition.ActorName);

        /// <summary>
        /// Answers a Mailbox for an Actor. If maybeMailbox is allocated answer it; otherwise
        /// answer a newly allocated Mailbox. (INTERNAL ONLY)
        /// </summary>
        /// <param name="definition">the Definition of the newly created Actor</param>
        /// <param name="address">the Address allocated to the Actor</param>
        /// <param name="maybeMailbox">the possible Mailbox</param>
        /// <returns></returns>
        private IMailbox AllocateMailbox(Definition definition, IAddress address, IMailbox? maybeMailbox)
            => maybeMailbox ?? ActorFactory.ActorMailbox(this, address, definition);

        /// <summary>
        /// Assert whether or not <paramref name="protocol"/> is an interface.
        /// </summary>
        /// <param name="protocol">Protocol that must be an interface.</param>
        private void AssertProtocolCompliance(Type protocol)
        {
            if (!protocol.IsInterface)
            {
                throw new ArgumentException($"Actor protocol must be an interface not a class: {protocol.Name}");
            }
        }

        /// <summary>
        /// Assert whether or not all of the <paramref name="protocols"/> are interfaces.
        /// </summary>
        /// <param name="protocols">Protocols that must all be interfaces.</param>
        private void AssertProtocolCompliance(ICollection<Type> protocols)
        {
            foreach(var protocol in protocols)
            {
                AssertProtocolCompliance(protocol);
            }
        }

        /// <summary>
        /// Answers a newly created Actor instance from the internal ActorFactory. (INTERNAL ONLY)
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="parent"></param>
        /// <param name="maybeAddress"></param>
        /// <param name="maybeMailbox"></param>
        /// <param name="maybeSupervisor"></param>
        /// <param name="logger"></param>
        /// <exception cref="InvalidOperationException" />
        /// <returns></returns>
        private Actor CreateRawActor(
          Definition definition,
          Actor? parent,
          IAddress? maybeAddress,
          IMailbox? maybeMailbox,
          ISupervisor? maybeSupervisor,
          ILogger logger)
        {
            if (IsStopped)
            {
                throw new InvalidOperationException("Actor stage has been stopped.");
            }

            var address = maybeAddress ?? World.AddressFactory.UniqueWith(definition.ActorName);
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
            catch (Exception e)
            {
                logger.Error($"Actor instantiation failed because: {e.Message}", e);

                throw new InvalidOperationException($"Actor instantiation failed because: {e.Message}", e);
            }

            directory.Register(actor.Address, actor);
            actor.LifeCycle.BeforeStart(actor);

            return actor;
        }

        /// <summary>
        /// Stops all Actor instances from the PrivateRootActor down to the last child. (INTERNAL ONLY)
        /// </summary>
        private void Sweep()
        {
            if (World.PrivateRoot != null)
            {
                World.PrivateRoot.Stop();
            }
        }
    }

    /// <summary>
    /// Internal type used to manage Actor proxy creation. (INTERNAL ONLY)
    /// </summary>
    /// <typeparam name="T">The protocol type.</typeparam>
    internal class ActorProtocolActor<T>
    {
        private readonly Actor actor;

        internal ActorProtocolActor(Actor actor, T protocol)
        {
            this.actor = actor;
            ProtocolActor = protocol;
        }

        internal T ProtocolActor { get; }

        internal static ActorProtocolActor<T>[] AllOf(T[] protocolActors, Actor actor)
        {
            var all = new ActorProtocolActor<T>[protocolActors.Length];
            for (int idx = 0; idx < protocolActors.Length; ++idx)
            {
                all[idx] = new ActorProtocolActor<T>(actor, protocolActors[idx]);
            }
            return all;
        }

        internal static object[] ToActors(ActorProtocolActor<object>[]? all)
        {
            if (all == null)
            {
                return new object[0];
            }
            
            var actors = new object[all.Length];
            for (int idx = 0; idx < all.Length; ++idx)
            {
                actors[idx] = all[idx].ProtocolActor;
            }
            return actors;
        }

        internal static object[] ToTestActors(ActorProtocolActor<T>[]? all, Type[] protocols)
        {
            if (all == null)
            {
                return new object[0];
            }
            
            var testActors = new object[all.Length];
            for (int idx = 0; idx < all.Length; ++idx)
            {
                testActors[idx] = all[idx].ToTestActor(protocols[idx]);
            }

            return testActors;
        }

        internal TestActor<T> ToTestActor() => new TestActor<T>(actor, ProtocolActor, actor.Address);

        private object ToTestActor(Type protocol)
        {
            var type = typeof(TestActor<>).MakeGenericType(protocol);
            return Activator.CreateInstance(type, actor, ProtocolActor, actor.Address);
        }
    }
}