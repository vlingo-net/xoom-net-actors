// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Actors.TestKit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ActorsTest : IDisposable
    {
        protected ActorsTest() 
        {
            var configuration = Configuration
                .Define()
                .With(ConsoleLoggerPluginConfiguration
                    .Define()
                    .WithDefaultLogger()
                    .WithName("vlingo-net/actors"));

            TestWorld = TestWorld.Start($"{GetType().Name}-world", configuration);
            World = TestWorld.World;
        }

        protected World World { get; set; }

        protected TestWorld TestWorld { get; set; }

        public TestUntil Until(int times) => TestUntil.Happenings(times);

        public virtual void Dispose() => TestWorld.Terminate();

        protected internal bool IsSuspended(Actor actor) => actor.LifeCycle.IsSuspended;
  }
}