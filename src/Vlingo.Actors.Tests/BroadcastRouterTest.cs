// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class BroadcastRouterTest : ActorsTest
    {
        [Fact]
        public void TestThreeArgConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var until = TestUntil.Happenings(messagesToSend);
            var testRouter = TestWorld.ActorFor<IThreeArgConsumerProtocol>(
                Definition.Has<MathCommandRouter>(Definition.Parameters(poolSize, until)));

            for (var round = 0; round < messagesToSend; ++round)
            {
                testRouter.Actor.DoSomeMath(round, round, round);
            }

            until.Completes();

            var routerActor = (MathCommandRouter)testRouter.ActorInside;
            foreach (var routee in routerActor.Routees)
            {
                Assert.Equal(messagesToSend, routee.MessageCount);
            }
        }

        private class MathCommandRouter : BroadcastRouter<IThreeArgConsumerProtocol>, IThreeArgConsumerProtocol
        {
            public MathCommandRouter(int poolSize, TestUntil testUntil) :
                base(new RouterSpecification(
                    poolSize,
                    Definition.Has<MathCommandWorker>(Definition.Parameters(testUntil)),
                    typeof(IThreeArgConsumerProtocol)))
            {
            }

            public void DoSomeMath(int arg1, int arg2, int arg3)
                => DispatchCommand(
                    (actor, a, b, c) => actor.DoSomeMath(a, b, c),
                    arg1,
                    arg2,
                    arg3);
        }

        private class MathCommandWorker : Actor, IThreeArgConsumerProtocol
        {
            private readonly TestUntil testUntil;

            public MathCommandWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void DoSomeMath(int arg1, int arg2, int arg3)
            {
                var sum = arg1 + arg2 + arg3;
                var product = arg1 * arg2 * arg3;
                testUntil.Happened();
            }
        }
    }

    public interface IThreeArgConsumerProtocol
    {
        void DoSomeMath(int arg1, int arg2, int arg3);
    }
}
