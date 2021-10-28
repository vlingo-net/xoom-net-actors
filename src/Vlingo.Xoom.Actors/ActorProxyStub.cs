// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    public class ActorProxyStub : ActorProxyBase
    {
        public ActorProxyStub(Actor actor) : base(null, Actors.Definition.SerializationProxy.From(actor.Definition), actor.Address)
        {
        }
    }
}