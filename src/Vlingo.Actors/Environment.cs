using System;
using System.Collections.Generic;

namespace Vlingo.Actors
{
    public class Environment
    {
        internal Address Address { get; }
        private List<Actor> Children { get; }
        internal Definition Definition { get; }
        private FailureMark FailureMark { get; }
        
        internal ILogger Logger { get; }
        internal IMailbox Mailbox { get; }
        internal ISupervisor MaybeSupervisor { get; }
        internal Actor Parent { get; }
        private IDictionary<Type, object> ProxyCache { get; }
        internal Stage Stage { get; }
        internal Stowage Stowage { get; }
        internal Stowage Suspended { get; }

        private readonly AtomicBoolean secured;
        private readonly AtomicBoolean stopped;

        internal Environment(
            Stage stage,
            Address address,
            Definition definition,
            Actor parent,
            IMailbox mailbox,
            ISupervisor maybeSupervisor,
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
            Children = new List<Actor>(0);
            ProxyCache = new Dictionary<Type, object>();
            Stowage = new Stowage();
            Suspended = new Stowage();

            secured = new AtomicBoolean(false);
            stopped = new AtomicBoolean(false);
        }

        internal void AddChild(Actor child)
        {
            Children.Add(child);
        }

        internal void CacheProxy<T>(T proxy)
        {
            ProxyCache.Add(proxy.GetType(), proxy);
        }

        internal T LookUpProxy<T>() => (T)ProxyCache[typeof(T)];

        internal bool IsSecured => secured.Get();

        internal void SetSecured()
        {
            secured.Set(true);
        }

        internal bool IsStopped => stopped.Get();

        internal void Stop()
        {
            if(stopped.CompareAndSet(false, true))
            {
                StopChildren();
                Suspended.Reset();
                Stowage.Reset();
                Mailbox.Close();
            }
        }
        
        private void StopChildren()
        {
            Children.ForEach(c => c.Stop());
            Children.Clear();
        }
    }
}
