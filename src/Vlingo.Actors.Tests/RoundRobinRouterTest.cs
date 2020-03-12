// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
    public class RoundRobinRouterTest : ActorsTest
    {
        [Fact]
        public void TestTwoArgConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var results = new Results(messagesToSend);

            var router = World.ActorFor<ITwoArgSupplierProtocol>(typeof(TestRouterActor), poolSize);

            for (var i = 0; i < messagesToSend; i++)
            {
                router
                  .ProductOf(i, i)
                  .AndThenConsume(answer => results.Access.WriteUsing("answers", answer));
            }

            var allExpected = new List<int>();

            for (var round = 0; round < messagesToSend; round++)
            {
                int expected = round * round;
                allExpected.Add(expected);
            }

            for (var round = 0; round < messagesToSend; round++)
            {
                int actual = results.Access.ReadFrom<int, int>("answers", round);
                Assert.True(allExpected.Remove(actual));
            }

            Assert.Empty(allExpected);
        }

        private class TestRouterActor : RoundRobinRouter<ITwoArgSupplierProtocol>, ITwoArgSupplierProtocol
        {
            public TestRouterActor(int poolSize) :
                base(new RouterSpecification<ITwoArgSupplierProtocol>(
                    poolSize,
                    Definition.Has<TestRouteeActor>(Definition.NoParameters)))
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
                    .WritingWith("answers", (int answer) => answers[index++] = answer)
                    .ReadingWith("answers", (int index) => answers[index]);

                return Access;
            }
        }
    }

    public interface ITwoArgSupplierProtocol
    {
        ICompletes<int> ProductOf(int arg1, int arg2);
    }
}
