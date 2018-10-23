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

namespace Vlingo.Actors.TestKit
{
    public class TestWorld : IDisposable
    {
        private static ThreadLocal<TestWorld> ThreadLocalInstance { get; } = new ThreadLocal<TestWorld>();
        internal static TestWorld Instance
        {
            get
            {
                return ThreadLocalInstance.Value;
            }
            set
            {
                ThreadLocalInstance.Value = value;
            }
        }

        private readonly IDictionary<int, List<IMessage>> actorMessages = new Dictionary<int, List<IMessage>>();

        public IList<IMessage> AllMessagesFor(Address address)
            => actorMessages.ContainsKey(address.Id) ? actorMessages[address.Id] : new List<IMessage>();

        public static TestWorld Start(string name)
        {
            var world = World.Start(name);
            return new TestWorld(world, name);
        }

        private static readonly object startNamePropMutex = new object();
        public static TestWorld Start(string name, Properties properties)
        {
            lock (startNamePropMutex)
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

        private static readonly object startWithDefaultMutex = new object();
        public static TestWorld StartWithDefaults(string name)
        {
            lock (startWithDefaultMutex)
            {
                return new TestWorld(World.Start(name, Configuration.Define()), name);
            }
        }

        public void Track(IMessage message)
        {
            var id = message.Actor.Address.Id;
            if (!actorMessages.ContainsKey(id))
            {
                actorMessages[id] = new List<IMessage>();
            }
            actorMessages[id].Add(message);
        }

        public TestActor<T> ActorFor<T>(Definition definition)
        {
            if (World.IsTerminated)
            {
                throw new InvalidOperationException("vlingo-net/actors: TestWorld has stopped.");
            }

            return World.Stage.TestActorFor<T>(definition);
        }
        
        public Protocols ActorFor(Definition definition, Type[] protocols)
        {
            if (World.IsTerminated)
            {
                throw new InvalidOperationException("vlingo-net/actors: TestWorld has stopped.");
            }

            return World.Stage.TestActorFor(definition, protocols);
        }

        public ILogger DefaultLogger => World.DefaultLogger;

        public ILogger Logger(string name) => World.Logger(name);

        public Stage Stage => World.Stage;

        public Stage StageNamed(string name) => World.StageNamed(name);

        public bool IsTerminated => World.IsTerminated;

        public void Terminate()
        {
            World.Terminate();
            Instance = null;
            actorMessages.Clear();
        }

        public World World { get; }

        public IMailboxProvider MailboxProvider { get; }

        public void ClearTrackedMessages()
        {
            actorMessages.Clear();
        }

        private TestWorld(World world, string name)
        {
            World = world;
            MailboxProvider = new TestMailboxPlugin(World);
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
