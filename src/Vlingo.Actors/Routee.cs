// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    /// <summary>
    /// Routee represents a potential target for for a routed message.
    /// </summary>
    public class Routee<P>
    {
        private readonly P @delegate;
        private readonly IAddressable? addressable;
        private long messageCount;

        internal static Routee<T> Of<T>(T actor, IAddressable addressable)
            => new Routee<T>(actor, addressable);

        internal static Routee<T> Of<T>(T actor)
            => new Routee<T>(actor, null);

        internal Routee(P actor, IAddressable? addressable)
        {
            @delegate = actor;
            this.addressable = addressable;
            messageCount = 0;
        }

        public virtual P Delegate => @delegate;

        internal virtual LifeCycle DelegateLifeCycle => addressable!.LifeCycle;

        public virtual IAddress Address => addressable!.Address;

        public virtual int PendingMessages
            => DelegateLifeCycle.Environment.Mailbox.PendingMessages;

        public virtual long MessageCount => messageCount;

        protected internal virtual void ReceiveCommand<T1>(Action<P, T1> consumer, T1 routable1)
        {
            messageCount++;
            consumer.Invoke(@delegate, routable1);
        }

        protected internal virtual void ReceiveCommand<T1, T2>(Action<P, T1, T2> consumer, T1 routable1, T2 routable2)
        {
            messageCount++;
            consumer.Invoke(@delegate, routable1, routable2);
        }

        protected internal virtual void ReceiveCommand<T1, T2, T3>(Action<P, T1, T2, T3> consumer, T1 routable1, T2 routable2, T3 routable3)
        {
            messageCount++;
            consumer.Invoke(@delegate, routable1, routable2, routable3);
        }

        protected internal virtual void ReceiveCommand<T1, T2, T3, T4>(Action<P, T1, T2, T3, T4> consumer, T1 routable1, T2 routable2, T3 routable3, T4 routable4)
        {
            messageCount++;
            consumer.Invoke(@delegate, routable1, routable2, routable3, routable4);
        }

        public virtual R ReceiveQuery<T1, R>(Func<P, T1, R> query, T1 routable1) where R : ICompletes
        {
            messageCount++;
            return query.Invoke(@delegate, routable1);
        }

        public virtual R ReceiveQuery<T1, T2, R>(Func<P, T1, T2, R> query, T1 routable1, T2 routable2) where R : ICompletes
        {
            messageCount++;
            return query.Invoke(@delegate, routable1, routable2);
        }

        public virtual R ReceiveQuery<T1, T2, T3, R>(Func<P, T1, T2, T3, R> query, T1 routable1, T2 routable2, T3 routable3) where R : ICompletes
        {
            messageCount++;
            return query.Invoke(@delegate, routable1, routable2, routable3);
        }

        public virtual R ReceiveQuery<T1, T2, T3, T4, R>(Func<P, T1, T2, T3, T4, R> query, T1 routable1, T2 routable2, T3 routable3, T4 routable4) where R : ICompletes
        {
            messageCount++;
            return query.Invoke(@delegate, routable1, routable2, routable3, routable4);
        }

        public override int GetHashCode() => @delegate == null ? 0 : @delegate.GetHashCode();

        public override string ToString() => $"Routee(actor={@delegate})";

        public override bool Equals(object obj)
        {
            if(this == obj)
            {
                return true;
            }
            if(obj == null)
            {
                return false;
            }
            if(GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Routee<P>)obj;
            
            if(@delegate == null)
            {
                if(other.@delegate != null)
                {
                    return false;
                }
            }
            else if (!@delegate.Equals(other.@delegate))
            {
                return false;
            }

            return true;
        }
    }
}
