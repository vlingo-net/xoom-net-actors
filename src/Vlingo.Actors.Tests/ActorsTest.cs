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
        protected readonly World world;
        protected readonly TestWorld testWorld;

        public TestUntil until;

        protected ActorsTest() 
        {
            testWorld = TestWorld.Start("test");
            world = testWorld.World;
        }

        public TestUntil Until(int times) => TestUntil.Happenings(times);

        public void Dispose() => testWorld.Terminate();

        protected bool IsSuspended(Actor actor) => actor.LifeCycle.IsSuspended;
  }
}