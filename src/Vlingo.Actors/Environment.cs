// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Environment
    {
        internal IAddress Address { get; }
        internal ConcurrentDictionary<long, Actor> Children { get; }
        internal IAddress? CompletesEventuallyAddress { get; private set; }
        internal Definition Definition { get; }
        internal FailureMark FailureMark { get; }

        internal ILogger Logger { get; }
        internal IMailbox Mailbox { get; }
        internal ISupervisor? MaybeSupervisor { get; }
        internal Actor? Parent { get; }
        private IDictionary<Type, object> ProxyCache { get; }
        internal Stage Stage { get; }
        internal Stowage Stowage { get; }
        internal Stowage Suspended { get; }

        private readonly AtomicBoolean _secured;
        private readonly AtomicBoolean _stopped;
        private Type[]? _stowageOverrides;

        public static Environment Of(Actor actor) => actor.LifeCycle.Environment;
        
        public int PendingMessages => Mailbox.PendingMessages;

        protected internal Environment(
            Stage stage,
            IAddress address,
            Definition definition,
            Actor? parent,
            IMailbox mailbox,
            ISupervisor? maybeSupervisor,
            ILogger logger)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (address.Id != World.PrivateRootId)
            {
                Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }
            else
            {
                Parent = parent;
            }
            Mailbox = mailbox ?? throw new ArgumentNullException(nameof(mailbox));
            MaybeSupervisor = maybeSupervisor;
            FailureMark = new FailureMark();
            Logger = logger;
            Children = new ConcurrentDictionary<long, Actor>();
            ProxyCache = new Dictionary<Type, object>(1);
            Stowage = new Stowage();
            _stowageOverrides = null;
            Suspended = new Stowage();

            _secured = new AtomicBoolean(false);
            _stopped = new AtomicBoolean(false);
        }

        internal void AddChild(Actor child) => Children.AddOrUpdate(child.Address.Id, _ => child, (id, _) => child);

        internal void RemoveChild(Actor child) => Children.TryRemove(child.Address.Id, out _);

        internal ICompletesEventually CompletesEventually(ResultCompletes completes)
        {
            if (CompletesEventuallyAddress == null)
            {
                var completesEventually = Stage.World.CompletesFor(completes.ClientCompletes());
                CompletesEventuallyAddress = completesEventually.Address;
                return completesEventually;
            }

            return Stage.World.CompletesFor(CompletesEventuallyAddress, completes.ClientCompletes());
        }

        internal void CacheProxy<T>(T proxy)
        {
            if (proxy != null) ProxyCache[proxy.GetType()] = proxy;
        }

        internal void CacheProxy(Type proxyType, object proxy) => ProxyCache[proxyType] = proxy;

        internal T LookUpProxy<T>()
        {
            var lookup = LookUpProxy(typeof(T));
            if (lookup != null && !lookup.Equals(null))
            {
                return (T) lookup;
            }

            return default!;
        }

        internal object? LookUpProxy(Type type)
            => ProxyCache.ContainsKey(type) ? ProxyCache[type] : null;

        internal bool IsSecured => _secured.Get();

        internal void SetSecured() => _secured.Set(true);

        internal void RemoveFromParent(Actor actor) => Parent?.LifeCycle.Environment.RemoveChild(actor);

        internal bool IsStopped => _stopped.Get();

        internal void Stop()
        {
            if (_stopped.CompareAndSet(false, true))
            {
                StopChildren();
                Suspended.Reset();
                Stowage.Reset();
                Mailbox.Close();
            }
        }

        internal bool IsStowageOverride(Type protocol)
        {
            if (_stowageOverrides != null)
            {
                return _stowageOverrides.Contains(protocol);
            }

            return false;
        }

        internal void StowageOverrides(params Type[] overrides) => _stowageOverrides = overrides;

        private void StopChildren()
        {
            foreach (var idChildPair in Children)
            {
                idChildPair.Value.Stop();
            }
            
            Children.Clear();
        }
    }
}
