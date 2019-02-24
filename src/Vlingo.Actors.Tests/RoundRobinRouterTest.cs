// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class RoundRobinRouterTest : ActorsTest
    {
        [Fact]
        public void TestTwoArgConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var until = TestUntil.Happenings(messagesToSend);

            var testRouter = TestWorld.ActorFor<ITwoArgSupplierProtocol>(typeof(TestRouterActor), poolSize);

            var answers = new int[messagesToSend];
            for (var i = 0; i < messagesToSend; i++)
            {
                var round = i;
                testRouter.Actor
                  .ProductOf(round, round)
                  .AndThenConsume(answer => answers[round] = answer)
                  .AndThenConsume(_ => until.Happened());
            }

            until.Completes();

            for (var round = 0; round < messagesToSend; round++)
            {
                int expected = round * round;
                int actual = answers[round];
                Assert.Equal(expected, actual);
            }
        }

        private class TestRouterActor : RoundRobinRouter<ITwoArgSupplierProtocol>, ITwoArgSupplierProtocol
        {
            public TestRouterActor(int poolSize) :
                base(new RouterSpecification(
                    poolSize,
                    Definition.Has<TestRouteeActor>(Definition.NoParameters),
                    typeof(ITwoArgSupplierProtocol)))
            {
            }

            public ICompletes<int> ProductOf(int arg1, int arg2)
                => DispatchQuery(
                    (actor, x, y) => actor.ProductOf(x, y),
                    arg1,
                    arg2);
        }

        private class TestRouteeActor : Actor, ITwoArgSupplierProtocol
        {
            public ICompletes<int> ProductOf(int arg1, int arg2)
                => Completes().With(arg1 * arg2);
        }
    }

    public interface ITwoArgSupplierProtocol
    {
        ICompletes<int> ProductOf(int arg1, int arg2);
    }
}
