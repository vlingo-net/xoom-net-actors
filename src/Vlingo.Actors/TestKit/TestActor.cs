// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
            Context = new TestContext();
        }

        public virtual T Actor { get; }

        public virtual TActor ActorAs<TActor>()
            => (TActor)(object)Actor;

        public virtual IAddress Address { get; }

        public virtual Actor ActorInside { get; }

        public virtual TestState ViewTestState() => ActorInside.ViewTestState();

        public virtual TestContext Context { get; }
    }
}
