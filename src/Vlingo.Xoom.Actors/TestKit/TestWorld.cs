// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Xoom.Actors.TestKit
{
    public class TestWorld : IDisposable
    {
        private IMailboxProvider _mailboxProvider;
        private static ThreadLocal<TestWorld> ThreadLocalInstance { get; } = new ThreadLocal<TestWorld>();
        internal static TestWorld Instance
        {
            get
            {
                return ThreadLocalInstance.Value!;
            }
            set
            {
                ThreadLocalInstance.Value = value!;
            }
        }

        private readonly ConcurrentDictionary<long, IList<IMessage>> _actorMessages;

        public static TestWorld Start(string name)
        {
            var world = World.Start(name);
            return new TestWorld(world, name);
        }

        private static readonly object StartNamePropMutex = new object();
        public static TestWorld Start(string name, Properties properties)
        {
            lock (StartNamePropMutex)
            {
                var world = World.Start(name, properties);
                return new TestWorld(world, name);
            }
        }

        public static TestWorld Start(string name, Configuration configuration)
        {
            var world = World.Start(name, configuration);
            return new TestWorld(world, name);
        }

        public static TestWorld Start(string name, ILoggerProvider loggerProvider)
            => new TestWorld(World.Start(name), name);

        public static TestWorld StartWith(World world)
            => new TestWorld(world, world.Name);

        private static readonly object StartWithDefaultMutex = new object();
        public static TestWorld StartWithDefaults(string name)
        {
            lock (StartWithDefaultMutex)
            {
                return new TestWorld(World.Start(name, Configuration.Define()), name);
            }
        }

        public TestActor<T>? ActorFor<T>(Type type, params object[] parameters)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo-net/actors: TestWorld has stopped.");
            }

            return World.Stage.TestActorFor<T>(type, parameters);
        }

        public TestActor<T>? ActorFor<T>(Definition definition)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo-net/actors: TestWorld has stopped.");
            }

            return World.Stage.TestActorFor<T>(definition);
        }

        public TestActor<T>? ActorFor<T>(Expression<Func<T>> factory) => ActorFor<T>(Definition.Has(factory));
        
        public TestActor<T>? ActorFor<T>(Expression<Func<T>> factory, string actorName) => ActorFor<T>(Definition.Has(factory, actorName));
        
        public Protocols ActorFor(Type[] protocols, Definition definition)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo-net/actors: TestWorld has stopped.");
            }

            return World.Stage.TestActorFor(protocols, definition);
        }

        public IList<IMessage> AllMessagesFor(IAddress address)
        {
            if(_actorMessages.TryGetValue(address.Id, out var all))
            {
                return all;
            }

            return new List<IMessage>();
        }

        public void Close()
        {
            if (!IsTerminated)
            {
                Terminate();
            }
        }

        public void ClearTrackedMessages() => _actorMessages.Clear();

        public ILogger DefaultLogger => World.DefaultLogger;

        public ILogger Logger(string name) => World.Logger(name);

        public Stage Stage => World.Stage;

        public Stage StageNamed(string name) => World.StageNamed(name);

        public bool IsTerminated => World.IsTerminated;

        public void Terminate()
        {
            World.Terminate();
            Instance = null!;
            _actorMessages.Clear();
        }

        public void Track(IMessage message)
        {
            var id = message.Actor.Address.Id;
            if (!_actorMessages.ContainsKey(id))
            {
                _actorMessages[id] = new List<IMessage>();
            }

            _actorMessages[id].Add(message);
        }

        public World World { get; }

        private TestWorld(World world, string name)
        {
            World = world;
            _mailboxProvider = new TestMailboxPlugin(World);
            _actorMessages = new ConcurrentDictionary<long, IList<IMessage>>();
            Instance = this;
        }

        public void Dispose()
        {
            if (!IsTerminated)
            {
                Terminate();
            }
        }
    }
}
