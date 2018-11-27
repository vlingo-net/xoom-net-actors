// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.TestKit
{
    public class TestActor<T> : ITestStateView
    {
        public TestActor(Actor actor, T protocol, IAddress address)
        {
            ActorInside = actor;
            Actor = protocol;
            Address = address;
        }

        public T Actor { get; }
        public IAddress Address { get; }
        public Actor ActorInside { get; }

        public TestState ViewTestState() => ActorInside.ViewTestState();
    }
}
