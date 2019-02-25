// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public abstract class ContentBasedRouter<P> : Router<P>
    {
        protected ContentBasedRouter(RouterSpecification specification) : base(specification)
        {
        }

        protected internal override Routing<P> ComputeRouting()
        {
            throw new InvalidOperationException("This router does not have a default routing. Please re-implement the routingFor method(s)");
        }
    }
}
