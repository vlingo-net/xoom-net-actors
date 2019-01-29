// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;

namespace Vlingo.Actors
{
    /// <summary>
    /// Routing is an ordered sequence of <see cref="Routee"/> that
    /// was computed by a <see cref="IRoutingStrategy"/> and whose elements
    /// will be the target of a message forwarded by a <see cref="Router"/>.
    /// </summary>
    public class Routing
    {
        public static Routing Empty() => new Routing();
        public static Routing With(Routee routee) => new Routing(new List<Routee> { routee });

        public static Routing With(Optional<Routee> routeeOrNull)
            => routeeOrNull.IsPresent ?
                With(routeeOrNull.Get()) :
                Empty();

        public static Routing With(IList<Routee> routees) => new Routing(routees);

        private readonly ArraySegment<Routee> routees;

        internal Routing() : this(null)
        {
        }

        internal Routing(IList<Routee> routees)
        {
            var routeesCollection = routees ?? new List<Routee>();
            this.routees = new ArraySegment<Routee>(routeesCollection.ToArray());
        }

        public virtual IReadOnlyList<Routee> Routees => routees;

        public virtual IList<TProtocol> RouteesAs<TProtocol>()
            => routees.Select(r => r.As<TProtocol>()).ToList();

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
