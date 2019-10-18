// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.


namespace Vlingo.Actors
{
    /// <summary>
    /// SmallestMailboxRouter
    /// </summary>
    public class SmallestMailboxRouter<P> : Router<P>
    {
        public SmallestMailboxRouter(RouterSpecification<P> specification) : base(specification)
        {
        }

        /// <summary>
        /// See <see cref="Router{P}.ComputeRouting"/>
        /// </summary>
        protected internal override Routing<P> ComputeRouting()
        {
            Routee<P>? least = null;
            var leastCount = int.MaxValue;

            foreach(var routee in Routees)
            {
                var count = routee.PendingMessages;
                if(count == 0)
                {
                    least = routee;
                    break;
                }

                if(count < leastCount)
                {
                    least = routee;
                    leastCount = count;
                }
            }

            return Routing.With(least);
        }
    }
}
