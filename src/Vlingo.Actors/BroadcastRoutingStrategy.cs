// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors
{
    /// <summary>
    /// BroadcastRoutingStrategy is a <see cref="IRoutingStrategy"/> that
    /// includes all pooled <c>IList&lt;<see cref="Routee"/>&gt; routees</c> in the <see cref="Routing"/>.
    /// </summary>
    public class BroadcastRoutingStrategy : RoutingStrategyAdapter
    {
        public BroadcastRoutingStrategy()
        {
        }

        protected override Routing ChooseRouteFor(IList<Routee> routees)
        {
            return Routing.With(routees);
        }
    }
}
