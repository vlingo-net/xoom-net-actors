// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors;

/// <summary>
/// RandomRouter
/// </summary>
public class RandomRouter<P> : Router<P>
{
    private readonly Random _random;

    public RandomRouter(RouterSpecification<P> specification, int seed) : this(specification, new Random(seed))
    {
    }
        
    public RandomRouter(RouterSpecification<P> specification, Random seededRandom) : base(specification) 
        => _random = seededRandom;

    protected internal override Routing<P> ComputeRouting() => Routing.With(NextRoutee());

    protected internal virtual Routee<P>? NextRoutee()
    {
        var index = _random.Next(routees.Count);
        return RouteeAt(index);
    }
}