// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    public class RoundRobinRouter<P> : Router<P>
    {
        private int _poolIndex;

        public RoundRobinRouter(RouterSpecification<P> specification) : base(specification) => _poolIndex = 0;

        internal int PoolIndex => _poolIndex;

        protected internal override Routing<P> ComputeRouting() => Routing.With(NextRoutee());

        protected internal virtual Routee<P> NextRoutee()
        {
            var routees = Routees;
            _poolIndex = IncrementAndGetPoolIndex() % routees.Count;
            return routees[_poolIndex];
        }

        private int IncrementAndGetPoolIndex()
        {
            _poolIndex = (_poolIndex == int.MaxValue) ? 0 : _poolIndex + 1;
            return _poolIndex;
        }
    }
}
