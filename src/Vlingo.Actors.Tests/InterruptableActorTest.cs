// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class InterruptableActorTest : ActorsTest
    {
        [Fact]
        public void TestInterruptionWithStop()
        {
            var interruptable = TestWorld.ActorFor<IInterruptable>(
                Definition.Has<InterruptableActor>(
                    Definition.NoParameters, "testStoppable"));
            for(var idx = 0; idx < 10; ++idx)
            {
                if(idx == 5)
                {
                    interruptable.Actor.Stop();
                }
                interruptable.Actor.DoThisOrThat();
            }

            Assert.Equal(6, TestWorld.AllMessagesFor(interruptable.Address).Count);
            Assert.Equal(5, interruptable.ViewTestState().ValueOf<int>("totalReceived"));
        }

        private class InterruptableActor : Actor, IInterruptable
        {
            private int totalReceived = 0;
            public void DoThisOrThat()
            {
                if (!IsStopped)
                {
                    ++totalReceived;
                }
            }
            public override TestState ViewTestState() => new TestState().PutValue("totalReceived", totalReceived);
        }
    }

    public interface IInterruptable : IStoppable
    {
        void DoThisOrThat();
    }
}
