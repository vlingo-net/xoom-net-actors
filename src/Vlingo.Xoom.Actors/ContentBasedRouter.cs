// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// ContentBasedRouter is a kind of <see cref="Router{P}"/> that considers the
    /// content of messages in computing a <see cref="Routing{P}"/>.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    public abstract class ContentBasedRouter<P> : Router<P>
    {
        protected ContentBasedRouter(RouterSpecification<P> specification) : base(specification)
        {
        }

        protected internal override Routing<P> ComputeRouting()
        {
            throw new InvalidOperationException("This router does not have a default routing. Please re-implement the routingFor method(s)");
        }
    }
}
