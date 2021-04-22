// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// Router is a kind of <see cref="Actor"/> that forwards a message
    /// to one or more other <c><see cref="Actor"/> actors</c> according to a
    /// computed <see cref="Routing"/>.
    /// </summary>
    public abstract class Router<P> : Actor
    {
        protected readonly IList<Routee<P>> routees;

        public Router(RouterSpecification<P> specification)
        {
            routees = new List<Routee<P>>();
            InitRoutees(specification);
        }

        protected internal virtual void InitRoutees(RouterSpecification<P> specification)
        {
            for (var i = 0; i < specification.InitialPoolSize; ++i)
            {
                var protocols = new Type[] { specification.RouterProtocol, typeof(IAddressable) };
                var two = ChildActorFor(protocols, specification.RouterDefinition);
                Subscribe(Routee<P>.Of(two.Get<P>(0), two.Get<IAddressable>(1)));
            }
        }

        protected internal virtual IList<Routee<P>> Routees => new ArraySegment<Routee<P>>(routees.ToArray());

        public virtual Routee<P>? RouteeAt(int index) => index < 0 || index >= routees.Count ? null : routees.ElementAt(index);

        protected internal virtual void Subscribe(Routee<P> routee)
        {
            if (!routees.Contains(routee))
            {
                routees.Add(routee);
            }
        }

        protected internal virtual void Subscribe(IList<Routee<P>> toSubscribe)
        {
            foreach (var routee in toSubscribe)
            {
                Subscribe(routee);
            }
        }

        protected internal virtual void Unsubscribe(Routee<P> routee) => routees.Remove(routee);

        protected internal virtual void Unsubscribe(IList<Routee<P>> toUnsubscribe)
        {
            foreach (var routee in toUnsubscribe)
            {
                Unsubscribe(routee);
            }
        }

        protected internal abstract Routing<P> ComputeRouting();

        protected internal virtual Routing<P> RoutingFor<T1>(T1 routable1) => ComputeRouting();

        protected internal virtual Routing<P> RoutingFor<T1, T2>(T1 routable1, T2 routable2) => ComputeRouting();

        protected internal virtual Routing<P> RoutingFor<T1, T2, T3>(T1 routable1, T2 routable2, T3 routable3) => ComputeRouting();

        protected internal virtual Routing<P> RoutingFor<T1, T2, T3, T4>(T1 routable1, T2 routable2, T3 routable3, T4 routable4)
            => ComputeRouting();

        protected internal virtual void DispatchCommand<T1>(Action<P, T1> action, T1 routable1)
            => RoutingFor(routable1).Routees.ToList().ForEach(routee => routee.ReceiveCommand(action, routable1));

        protected internal virtual void DispatchCommand<T1, T2>(Action<P, T1, T2> action, T1 routable1, T2 routable2)
            => RoutingFor(routable1, routable2).Routees.ToList().ForEach(routee => routee.ReceiveCommand(action, routable1, routable2));

        protected internal virtual void DispatchCommand<T1, T2, T3>(Action<P, T1, T2, T3> action, T1 routable1, T2 routable2, T3 routable3)
            => RoutingFor(routable1, routable2, routable3)
                .Routees
                .ToList()
                .ForEach(routee => routee.ReceiveCommand(action, routable1, routable2, routable3));

        protected internal virtual void DispatchCommand<T1, T2, T3, T4>(
            Action<P, T1, T2, T3, T4> action,
            T1 routable1,
            T2 routable2,
            T3 routable3,
            T4 routable4)
            => RoutingFor(routable1, routable2, routable3, routable4)
                .Routees
                .ToList()
                .ForEach(routee => routee.ReceiveCommand(action, routable1, routable2, routable3, routable4));

        protected internal virtual ICompletes<R> DispatchQuery<T1, R>(
            Func<P, T1, ICompletes<R>> query,
            T1 routable1)
        {
            var completesEventually = CompletesEventually();
            RoutingFor(routable1)
                .First
                .ReceiveQuery(query, routable1)
                .AndThenConsume(outcome => completesEventually.With(outcome!));

            return (ICompletes<R>)Completes();
        }

        protected internal virtual ICompletes<R> DispatchQuery<T1, T2, R>(
            Func<P, T1, T2, ICompletes<R>> query,
            T1 routable1,
            T2 routable2)
        {
            var completesEventually = CompletesEventually();
            RoutingFor(routable1, routable2)
                .First
                .ReceiveQuery(query, routable1, routable2)
                .AndThenConsume(outcome => completesEventually.With(outcome!));

            return (ICompletes<R>)Completes();
        }

        protected internal virtual ICompletes<R> DispatchQuery<T1, T2, T3, R>(
            Func<P, T1, T2, T3, ICompletes<R>> query,
            T1 routable1,
            T2 routable2,
            T3 routable3)
        {
            var completesEventually = CompletesEventually();
            RoutingFor(routable1, routable2, routable3)
                .First
                .ReceiveQuery(query, routable1, routable2, routable3)
                .AndThenConsume(outcome => completesEventually.With(outcome!));

            return (ICompletes<R>)Completes();
        }

        protected internal virtual ICompletes<R> DispatchQuery<T1, T2, T3, T4, R>(
            Func<P, T1, T2, T3, T4, ICompletes<R>> query,
            T1 routable1,
            T2 routable2,
            T3 routable3,
            T4 routable4)
        {
            var completesEventually = CompletesEventually();
            RoutingFor(routable1, routable2, routable3, routable4)
                .First
                .ReceiveQuery(query, routable1, routable2, routable3, routable4)
                .AndThenConsume(outcome => completesEventually.With(outcome!));

            return (ICompletes<R>)Completes();
        }
    }
}
