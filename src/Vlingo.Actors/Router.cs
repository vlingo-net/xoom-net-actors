// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors
{
    public abstract class Router : Actor
    {
        private readonly IList<Routee> routees;
        private readonly IRoutingStrategy routingStrategy;

        protected internal Router(RouterSpecification specification, IRoutingStrategy routingStrategy)
        {
            for (int i = 0; i < specification.poolSize(); i++)
            {
                ChildActorFor(specification.RouterDefinition, specification.RouterProtocol);
            }
            this.routees = Routee.ForAll(LifeCycle.Environment.Children);
            this.routingStrategy = routingStrategy;
        }

        protected internal virtual Routing ComputeRouting<T1>(T1 routable1)
        {
            var routing = routingStrategy.ChooseRouteFor(routable1, routees);
            routing.Validate();
            return routing;
        }

        protected internal virtual Routing ComputeRouting<T1, T2>(T1 routable1, T2 routable2)
        {
            var routing = routingStrategy.ChooseRouteFor(routable1, routable2, routees);
            routing.Validate();
            return routing;
        }

        protected internal virtual Routing ComputeRouting<T1, T2, T3>(T1 routable1, T2 routable2, T3 routable3)
        {
            var routing = routingStrategy.ChooseRouteFor(routable1, routable2, routable3, routees);
            routing.Validate();
            return routing;
        }

        protected internal virtual Routing ComputeRouting<T1, T2, T3, T4>(T1 routable1, T2 routable2, T3 routable3, T4 routable4)
        {
            var routing = routingStrategy.ChooseRouteFor(routable1, routable2, routable3, routable4, routees);
            routing.Validate();
            return routing;
        }

        protected internal virtual Routing ComputeRouting<T1, T2, T3, T4, T5>(T1 routable1, T2 routable2, T3 routable3, T4 routable4, T5 routable5)
        {
            var routing = routingStrategy.ChooseRouteFor(routable1, routable2, routable3, routable4, routable5, routees);
            routing.Validate();
            return routing;
        }
    }
}
