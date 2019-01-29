// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Xunit;
using static Vlingo.Actors.Tests.DeadLettersTest;

namespace Vlingo.Actors.Tests
{
    public class DeadLettersTest : IDisposable
    {
        private readonly TestWorld world;

        public DeadLettersTest()
        {
            world = TestWorld.Start("test-dead-letters");
        }

        public void Dispose() => world.Terminate();

        [Fact]
        public void TestStoppedActorToDeadLetters()
        {
            var result = new TestResult(3);
            var nothing = world.ActorFor<INothing>(Definition.Has<NothingActor>(Definition.NoParameters, "nothing"));
            var listener = world.ActorFor<IDeadLettersListener>(
                Definition.Has<DeadLettersListenerActor>(
                    Definition.Parameters(result), "deadletters-listener"));
            world.World.DeadLetters.RegisterListener(listener.Actor);

            nothing.Actor.Stop();
            nothing.Actor.DoNothing(1);
            nothing.Actor.DoNothing(2);
            nothing.Actor.DoNothing(3);

            result.until.Completes();

            Assert.Equal(3, result.deadLetters.Count);

            foreach (var deadLetter in result.deadLetters)
            {
                Assert.Equal("DoNothing(int)", deadLetter.Representation);
            }
        }

        public class TestResult
        {
            public readonly IList<DeadLetter> deadLetters;
            public readonly TestUntil until;

            public TestResult(int happenings)
            {
                deadLetters = new List<DeadLetter>();
                until = TestUntil.Happenings(happenings);
            }
        }
    }

    public interface INothing : IStoppable
    {
        void DoNothing(int val);
    }

    public class NothingActor : Actor, INothing
    {
        public void DoNothing(int val)
        {
        }
    }

    public class DeadLettersListenerActor : Actor, IDeadLettersListener
    {
        private readonly TestResult result;

        public DeadLettersListenerActor(TestResult result)
        {
            this.result = result;
        }

        public void Handle(DeadLetter deadLetter)
        {
            result.deadLetters.Add(deadLetter);
            result.until.Happened();
        }
    }
}
