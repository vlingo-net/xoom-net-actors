// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;
using static Vlingo.Xoom.Actors.Tests.DeadLettersTest;

namespace Vlingo.Xoom.Actors.Tests
{
    public class DeadLettersTest : ActorsTest
    {
        [Fact]
        public void TestStoppedActorToDeadLetters()
        {
            var result = new TestResult(3);
            var nothing = TestWorld.ActorFor<INothing>(Definition.Has<NothingActor>(Definition.NoParameters, "nothing"));
            var listener = TestWorld.ActorFor<IDeadLettersListener>(
                Definition.Has<DeadLettersListenerActor>(
                    Definition.Parameters(result), "deadletters-listener"));
            World.DeadLetters.RegisterListener(listener.Actor);

            nothing.Actor.Stop();
            nothing.Actor.DoNothing(1);
            nothing.Actor.DoNothing(2);
            nothing.Actor.DoNothing(3);

            var deadLetters = result.GetDeadLetters();
            Assert.Equal(3, deadLetters.Count);

            foreach (var deadLetter in deadLetters)
            {
                Assert.Equal("DoNothing(int)", deadLetter.Representation);
            }
        }

        public class TestResult
        {
            internal readonly AccessSafely deadLetters;

            public TestResult(int happenings)
            {
                var dls = new ConcurrentBag<DeadLetter>();
                deadLetters = AccessSafely.AfterCompleting(happenings);
                deadLetters.WritingWith<DeadLetter>("dl", x => dls.Add(x));
                deadLetters.ReadingWith("dl", () => dls.ToArray());
            }

            public ICollection<DeadLetter> GetDeadLetters() => deadLetters.ReadFrom<ICollection<DeadLetter>>("dl");
            public void AddDeadLetter(DeadLetter deadLetter) => deadLetters.WriteUsing("dl", deadLetter);
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

        public void Handle(DeadLetter deadLetter) => result.AddDeadLetter(deadLetter);
    }
}
