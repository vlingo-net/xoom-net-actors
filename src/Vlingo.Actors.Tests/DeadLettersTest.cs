// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class DeadLettersTest : ActorsTest
    {
        private readonly TestActor<IDeadLettersListener> listener;
        private readonly TestActor<INothing> nothing;

        public DeadLettersTest()
            : base()
        {
            nothing = TestWorld.ActorFor<INothing>(Definition.Has<NothingActor>(Definition.NoParameters, "nothing"));
            listener = TestWorld.ActorFor<IDeadLettersListener>(Definition.Has<DeadLettersListenerActor>(Definition.NoParameters, "deadletters-listener"));
            TestWorld.World.DeadLetters.RegisterListener(listener.Actor);
        }

        [Fact]
        public void TestStoppedActorToDeadLetters()
        {
            nothing.Actor.Stop();
            nothing.Actor.DoNothing(1);
            nothing.Actor.DoNothing(2);
            nothing.Actor.DoNothing(3);
            DeadLettersListenerActor.WaitForExpectedMessages(3);

            Assert.Equal(3, DeadLettersListenerActor.deadLetters.Count);
            foreach (var deadLetter in DeadLettersListenerActor.deadLetters)
            {
                Assert.Equal("DoNothing(int)", deadLetter.Representation);
            }
        }
    }

    public interface INothing : IStoppable
    {
        void DoNothing(int withValue);
    }

    public class NothingActor : Actor, INothing
    {
        public void DoNothing(int withValue)
        {
        }
    }

    public class DeadLettersListenerActor : Actor, IDeadLettersListener
    {
        internal static List<DeadLetter> deadLetters = new List<DeadLetter>();

        public void Handle(DeadLetter deadLetter)
        {
            deadLetters.Add(deadLetter);
        }

        internal static void WaitForExpectedMessages(int count)
        {
            for (int idx = 0; idx < 1000; ++idx)
            {
                if (deadLetters.Count >= count)
                {
                    return;
                }
                else
                {
                    try { Thread.Sleep(10); } catch (Exception) { }
                }
            }
        }
    }
}
