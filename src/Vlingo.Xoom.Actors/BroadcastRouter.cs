// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Actors
{
    /// <summary>
    /// BrodcastRouter
    /// </summary>
    /// <typeparam name="P"></typeparam>
    public abstract class BroadcastRouter<P> : Router<P>
    {
        protected internal BroadcastRouter(RouterSpecification<P> specification) : base(specification)
        {
        }

        /// <summary>
        /// See <see cref="Router{P}.ComputeRouting"/>
        /// </summary>
        protected internal override Routing<P> ComputeRouting() => Routing.With(Routees);

        /// <summary>
        /// See <see cref="Router{P}.DispatchQuery{T1, R}(Func{P, T1, ICompletes{R}}, T1)"/>
        /// </summary>
        protected internal override ICompletes<R> DispatchQuery<T1, R>(System.Func<P, T1, ICompletes<R>> query, T1 routable1)
            => throw new InvalidOperationException("query protocols are not supported by this router by default");

        /// <summary>
        /// See <see cref="Router{P}.DispatchQuery{T1, T2, R}(Func{P, T1, T2, ICompletes{R}}, T1, T2)"/>
        /// </summary>
        protected internal override ICompletes<R> DispatchQuery<T1, T2, R>(Func<P, T1, T2, ICompletes<R>> query, T1 routable1, T2 routable2)
            => throw new InvalidOperationException("query protocols are not supported by this router by default");

        /// <summary>
        /// See <see cref="Router{P}.DispatchQuery{T1, T2, T3, R}(Func{P, T1, T2, T3, ICompletes{R}}, T1, T2, T3)"/>
        /// </summary>
        protected internal override ICompletes<R> DispatchQuery<T1, T2, T3, R>(Func<P, T1, T2, T3, ICompletes<R>> query, T1 routable1, T2 routable2, T3 routable3)
            => throw new InvalidOperationException("query protocols are not supported by this router by default");

        /// <summary>
        /// See <see cref="Router{P}.DispatchQuery{T1, T2, T3, T4, R}(Func{P, T1, T2, T3, T4, ICompletes{R}}, T1, T2, T3, T4)"/>
        /// </summary>
        protected internal override ICompletes<R> DispatchQuery<T1, T2, T3, T4, R>(Func<P, T1, T2, T3, T4, ICompletes<R>> query, T1 routable1, T2 routable2, T3 routable3, T4 routable4)
            => throw new InvalidOperationException("query protocols are not supported by this router by default");
    }
}
