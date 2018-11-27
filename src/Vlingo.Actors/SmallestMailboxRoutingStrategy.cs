// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors
{
    public class SmallestMailboxRoutingStrategy : RoutingStrategyAdapter
    {
        protected override Routing ChooseRouteFor(IList<Routee> routees)
        {
            Routee least = null;
            int leastCount = int.MaxValue;
            foreach (var routee in routees)
            {
                var count = routee.PendingMessages;
                if (count == 0)
                {
                    least = routee;
                    break;
                }
                else if (count < leastCount)
                {
                    least = routee;
                    leastCount = count;
                }
            }

            return least == null ? Routing.Empty() : Routing.With(least);
        }
    }
}
