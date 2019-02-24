// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public class RoundRobinRouter<P> : Router<P>
    {
        private int poolIndex;

        public RoundRobinRouter(RouterSpecification specification) : base(specification)
        {
            poolIndex = 0;
        }

        protected internal override Routing<P> ComputeRouting() => Routing.With(NextRoutee());

        protected internal virtual Routee<P> NextRoutee()
        {
            var routees = Routees;
            poolIndex = poolIndex++ % routees.Count;
            return routees[poolIndex];
        }
    }
}
