// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors
{
    /// <summary>
    /// RoundRobinRoutingStrategy is a <see cref="IRoutingStrategy"/> that
    /// treats its pool of <c>IList&lt;<see cref="Routee"/>&gt; routees</c> as if it were a
    /// circular linked list and which includes each routee, in turn,
    /// in the <see cref="Routing"/>.
    /// </summary>
    public class RoundRobinRoutingStrategy : RoutingStrategyAdapter
    {
        private int lastIndex;

        public RoundRobinRoutingStrategy()
        {
            lastIndex = 0;
        }

        protected override Routing ChooseRouteFor(IList<Routee> routees)
        {
            var nextIndex = lastIndex++ % routees.Count;
            return Routing.With(routees[nextIndex]);
        }
    }
}
