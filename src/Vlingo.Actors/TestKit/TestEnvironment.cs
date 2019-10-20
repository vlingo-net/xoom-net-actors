// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Actors.TestKit
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
