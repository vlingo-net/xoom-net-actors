using System;
using System.Collections.Generic;

namespace Vlingo.Actors
{
    public class Environment
    {
        private const byte FLAG_RESET = 0x00;
        private const byte FLAG_STOPPED = 0x01;
        private const byte FLAG_SECURED = 0x02;

        private byte _flags;

        internal Address Address { get; }
        private List<Actor> Children { get; }
        private Definition Definition { get; }
        private FailureMark FailureMark { get; }
        
        internal Logger Logger { get; }
        internal IMailbox Mailbox { get; }
        private ISupervisor MaybeSupervisor { get; }
        internal Actor Parent { get; }
        private IDictionary<Type, object> ProxyCache { get; }
        internal Stage Stage { get; }
        internal Stowage Stowage { get; }
        private Stowage Suspended { get; }

        internal Environment(
            Stage stage,
            Address address,
            Definition definition,
            Actor parent,
            IMailbox mailbox,
            ISupervisor maybeSupervisor,
            Logger logger)
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

            _flags = FLAG_RESET;
        }

        internal void AddChild(Actor child)
        {
            Children.Add(child);
        }

        private void CacheProxy<T>(T proxy)
        {
            ProxyCache.Add(proxy.GetType(), proxy);
        }

        private T LookUpProxy<T>() => (T)ProxyCache[typeof(T)];

        internal bool IsSecured => (_flags & FLAG_SECURED) == FLAG_SECURED;

        internal void SetSecured()
        {
            _flags |= FLAG_SECURED;
        }

        internal bool IsStopped => (_flags & FLAG_STOPPED) == FLAG_STOPPED;

        internal void Stop()
        {
            StopChildren();
            Suspended.Reset();
            Stowage.Reset();
            Mailbox.Close();
            SetStopped();
        }

        private void SetStopped()
        {
            _flags |= FLAG_STOPPED;
        }

        private void StopChildren()
        {
            Children.ForEach(c => c.Stop());
            Children.Clear();
        }
    }
}
