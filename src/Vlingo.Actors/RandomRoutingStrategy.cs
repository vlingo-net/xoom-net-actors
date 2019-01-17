// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors
{
    /// <summary>
    /// RandomRoutingStrategy is a <see cref="IRoutingStrategy"/> that
    /// includes a random one of the pooled <c>IList&lt;<see cref="Routee"/>&gt; routees</c>
    /// in the <see cref="Routing"/>
    /// </summary>
    public class RandomRoutingStrategy : RoutingStrategyAdapter
    {
        private readonly Random random;

        public RandomRoutingStrategy()
        {
            random = new Random();
        }

        protected override Routing ChooseRouteFor(IList<Routee> routees)
        {
            int index = random.Next(routees.Count);
            return Routing.With(routees[index]);
        }
    }
}
