// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    /// <summary>
    /// RandomRouter
    /// </summary>
    public class RandomRouter<P> : Router<P>
    {
        private readonly Random random;

        public RandomRouter(RouterSpecification<P> specification) : base(specification)
        {
            random = new Random();
        }

        protected internal override Routing<P> ComputeRouting() => Routing.With(NextRoutee());

        protected internal virtual Routee<P>? NextRoutee()
        {
            int index = random.Next(routees.Count);
            return RouteeAt(index);
        }
    }
}
