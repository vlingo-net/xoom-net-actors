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
    public class RandomRouterTest : ActorsTest
    {
        [Fact]
        public void TestSupplierProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var until = TestUntil.Happenings(messagesToSend);

            var testRouter = TestWorld.ActorFor<IOneArgSupplierProtocol>(typeof(TestSupplierActor), poolSize);
            var answers = new int[messagesToSend];
            for (var i = 0; i < messagesToSend; i++)
            {
                var round = i;
                testRouter.Actor
                  .CubeOf(round)
                  .AndThenConsume(answer => answers[round] = answer)
                  .AndThenConsume(_ => until.Happened());
            }

            until.Completes();

            for (var round = 0; round < messagesToSend; round++)
            {
                int expected = round * round * round;
                int actual = answers[round];
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void TestConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var until = TestUntil.Happenings(messagesToSend);

            var testRouter = TestWorld.ActorFor<IOneArgConsumerProtocol>(typeof(TestConsumerActor), poolSize, until);

            for (var i = 0; i < messagesToSend; i++)
            {
                testRouter.Actor.Remember(i);
            }

            until.Completes();
        }


        private class TestSupplierActor : RoundRobinRouter<IOneArgSupplierProtocol>, IOneArgSupplierProtocol
        {
            public TestSupplierActor(int poolSize) :
                base(new RouterSpecification(
                    poolSize,
                    Definition.Has<TestSupplierWorker>(Definition.NoParameters),
                    typeof(IOneArgSupplierProtocol)))
            {
            }

            public ICompletes<int> CubeOf(int arg1)
                => DispatchQuery(
                    (actor, x) => actor.CubeOf(x),
                    arg1);
        }

        private class TestSupplierWorker : Actor, IOneArgSupplierProtocol
        {
            public ICompletes<int> CubeOf(int arg1)
                => Completes().With(arg1 * arg1 * arg1);
        }

        private class TestConsumerActor : RoundRobinRouter<IOneArgConsumerProtocol>, IOneArgConsumerProtocol
        {
            public TestConsumerActor(int poolSize, TestUntil testUntil) :
                base(new RouterSpecification(
                    poolSize,
                    Definition.Has<TestConsumerWorker>(Definition.Parameters(testUntil)),
                    typeof(IOneArgConsumerProtocol)))
            {
            }

            public void Remember(int number)
                => DispatchCommand(
                    (actor, x) => actor.Remember(x),
                    number);
        }

        private class TestConsumerWorker : Actor, IOneArgConsumerProtocol
        {
            private readonly TestUntil testUntil;

            public TestConsumerWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void Remember(int number) => testUntil.Happened();
        }
    }

    public interface IOneArgSupplierProtocol
    {
        ICompletes<int> CubeOf(int arg1);
    }

    public interface IOneArgConsumerProtocol
    {
        void Remember(int number);
    }
}
