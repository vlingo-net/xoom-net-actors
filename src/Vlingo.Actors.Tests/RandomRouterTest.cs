// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
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
            var results = new Results(messagesToSend);

            var router = World.ActorFor<IOneArgSupplierProtocol>(Definition.Has<TestSupplierActor>(Definition.Parameters(poolSize)));
            for (var i = 0; i < messagesToSend; i++)
            {
                var round = i;
                router
                  .CubeOf(round)
                  .AndThenConsume(answer => results.Access.WriteUsing("answers", answer));
            }

            var allExpected = new List<int>();

            for (var round = 0; round < messagesToSend; round++)
            {
                int expected = round * round * round;
                allExpected.Add(expected);
            }

            for (var round = 0; round < messagesToSend; round++)
            {
                int actual = results.Access.ReadFrom<int, int>("answers", round);
                Assert.True(allExpected.Remove(actual));
            }

            Assert.Empty(allExpected);
        }

        [Fact]
        public void TestConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var results = new Results(messagesToSend);

            var router = World.ActorFor<IOneArgConsumerProtocol>(typeof(TestConsumerActor), results, poolSize);

            for (var i = 0; i < messagesToSend; i++)
            {
                router.Remember(i);
            }

            var allExpected = new List<int>();

            for (var round = 0; round < messagesToSend; round++)
            {
                allExpected.Add(round);
            }

            for (var round = 0; round < messagesToSend; round++)
            {
                Assert.True(allExpected.Remove(round));
            }

            Assert.Empty(allExpected);
        }


        private class TestSupplierActor : RoundRobinRouter<IOneArgSupplierProtocol>, IOneArgSupplierProtocol
        {
            public TestSupplierActor(int poolSize) :
                base(new RouterSpecification<IOneArgSupplierProtocol>(
                    poolSize,
                    Definition.Has<TestSupplierWorker>(Definition.NoParameters)))
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
            public TestConsumerActor(Results results, int poolSize) :
                base(new RouterSpecification<IOneArgConsumerProtocol>(
                    poolSize,
                    Definition.Has<TestConsumerWorker>(Definition.Parameters(results))))
            {
            }

            public void Remember(int number)
                => DispatchCommand(
                    (actor, x) => actor.Remember(x),
                    number);
        }

        private class TestConsumerWorker : Actor, IOneArgConsumerProtocol
        {
            private readonly Results results;

            public TestConsumerWorker(Results results)
            {
                this.results = results;
            }

            public void Remember(int number) => results.Access.WriteUsing("answers", number);
        }

        private class Results
        {
            private readonly int[] answers;
            private int index;

            public AccessSafely Access { get; private set; }

            public Results(int totalAnswers)
            {
                answers = new int[totalAnswers];
                index = 0;
                Access = AfterCompleting(totalAnswers);
            }

            private AccessSafely AfterCompleting(int steps)
            {
                Access = AccessSafely
                    .AfterCompleting(steps)
                    .WritingWith<int>("answers", answer => answers[index++] = answer)
                    .ReadingWith<int, int>("answers", indx => answers[indx]);

                return Access;
            }
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
