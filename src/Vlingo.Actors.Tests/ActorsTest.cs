// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests
{
    public class ActorsTest : IDisposable
    {
        protected ActorsTest() 
        {
            TestWorld = TestWorld.Start($"{GetType().Name}-world");
            World = TestWorld.World;
            //ActorFactory.ThreadLocalEnvironment.Value = new TestEnvironment();
        }

        protected World World { get; }

        protected TestWorld TestWorld { get; }

        public TestUntil Until(int times) => TestUntil.Happenings(times);

        public virtual void Dispose() => TestWorld.Terminate();

        protected internal bool IsSuspended(Actor actor) => actor.LifeCycle.IsSuspended;
  }
}