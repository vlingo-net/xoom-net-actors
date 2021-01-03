// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    /// <summary>
    /// See <see cref="Routing{P}"/>
    /// </summary>
    public static class Routing
    {
        public static Routing<T> With<T>(Routee<T>? routee)
            => new Routing<T>(new List<Routee<T>> {
                routee ?? throw new ArgumentNullException(nameof(routee), "Routee may not be null")
            });

        public static Routing<T> With<T>(ICollection<Routee<T>> routees)
        {
            if (routees == null || routees.Count == 0)
            {
                throw new ArgumentNullException(nameof(routees), "routees may not be null or empty");
            }

            return new Routing<T>(routees);
        }
    }

    /// <summary>
    /// Routing is an ordered sequence of <see cref="Routee"/> that
    /// was computed by way of some routing strategy and whose elements
    /// will be the target of a message forwarded by a <see cref="Router"/>.
    /// </summary>
    public class Routing<P>
    {
        private readonly ArraySegment<Routee<P>> routees;

        internal Routing() : this(null)
        {
        }

        internal Routing(ICollection<Routee<P>>? routees)
        {
            var routeesCollection = routees ?? new List<Routee<P>>();
            this.routees = new ArraySegment<Routee<P>>(routeesCollection.ToArray());
        }

        public virtual Routee<P> First => routees.ElementAt(0);

        public virtual IReadOnlyList<Routee<P>> Routees => routees;

        public virtual bool IsEmpty => routees.Count == 0;

        public override string ToString() => $"Routing[routees={routees}]";

        public virtual void Validate()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("routees may not be empty");
            }
        }
    }
}
