// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors
{
    public abstract class RoutingStrategyAdapter : IRoutingStrategy
    {
        public virtual Routing ChooseRouteFor<T1>(T1 routable1, IList<Routee> routees)
            => ChooseRouteFor(routees);

        public virtual Routing ChooseRouteFor<T1, T2>(T1 routable1, T2 routable2, IList<Routee> routees)
            => ChooseRouteFor(routees);

        public virtual Routing ChooseRouteFor<T1, T2, T3>(T1 routable1, T2 routable2, T3 routable3, IList<Routee> routees)
            => ChooseRouteFor(routees);

        public virtual Routing ChooseRouteFor<T1, T2, T3, T4>(T1 routable1, T2 routable2, T3 routable3, T4 routable4, IList<Routee> routees)
            => ChooseRouteFor(routees);

        public virtual Routing ChooseRouteFor<T1, T2, T3, T4, T5>(T1 routable1, T2 routable2, T3 routable3, T4 routable4, T5 routable5, IList<Routee> routees)
            => ChooseRouteFor(routees);

        protected abstract Routing ChooseRouteFor(IList<Routee> routees);
    }
}
