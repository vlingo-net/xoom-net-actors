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
    /// RoutingStrategy is an object that knows how to compute a
    /// <see cref="Routing"/> for a message based on a defined strategy
    /// (e.g., round robin, smallest mailbox, etc.). An empty <see cref="Routing"/>
    /// is not legal and will result in an <c>InvalidOperationException</c>.
    /// </summary>
    public interface IRoutingStrategy
    {
        Routing ChooseRouteFor<T1>(T1 routable1, IList<Routee> routees);
        Routing ChooseRouteFor<T1, T2>(T1 routable1, T2 routable2, IList<Routee> routees);
        Routing ChooseRouteFor<T1, T2, T3>(T1 routable1, T2 routable2, T3 routable3, IList<Routee> routees);
        Routing ChooseRouteFor<T1, T2, T3, T4>(T1 routable1, T2 routable2, T3 routable3, T4 routable4, IList<Routee> routees);
        Routing ChooseRouteFor<T1, T2, T3, T4, T5>(T1 routable1, T2 routable2, T3 routable3, T4 routable4, T5 routable5, IList<Routee> routees);
    }
}
