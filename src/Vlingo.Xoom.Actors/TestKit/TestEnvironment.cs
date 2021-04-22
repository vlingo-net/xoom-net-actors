// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Xoom.Actors.TestKit
{
    internal class TestEnvironment : Environment
    {
        public TestEnvironment() :
            base(
                TestWorld.Instance!.World.Stage,
                TestWorld.Instance.World.AddressFactory.UniqueWith("test"),
                Definition.Has<Actor>(Definition.NoParameters),
                TestWorld.Instance.World.DefaultParent!,
                new TestMailbox(),
                TestWorld.Instance.World.DefaultSupervisor,
                TestWorld.Instance.World.DefaultLogger)
        {
        }
    }
}
