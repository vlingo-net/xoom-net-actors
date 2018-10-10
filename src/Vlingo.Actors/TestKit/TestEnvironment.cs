// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Actors.TestKit
{
    public class TestEnvironment : Environment
    {
        public TestEnvironment() :
            base(
                TestWorld.testWorld.World.Stage,
                TestWorld.testWorld.World.AddressFactory.UniqueWith("test"),
                Definition.Has<Actor>(Definition.NoParameters),
                TestWorld.testWorld.World.DefaultParent,
                new TestMailbox(),
                TestWorld.testWorld.World.DefaultSupervisor,
                ConsoleLogger.TestInstance())
        {
                
        }
    }
}
