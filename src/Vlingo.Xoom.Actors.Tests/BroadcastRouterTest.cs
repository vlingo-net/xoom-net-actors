// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class BroadcastRouterTest : ActorsTest
    {
        [Fact]
        public void TestThreeArgConsumerProtocol()
        {
            var poolSize = 4;
            var rounds = 2;
            var messagesToSend = poolSize * rounds;
            var totalMessagesExpected = messagesToSend * poolSize;

            var testResults = TestResults.AfterCompleting(totalMessagesExpected);

            var testRouter = TestWorld.ActorFor<IThreeArgConsumerProtocol>(
                Definition.Has<MathCommandRouter>(Definition.Parameters(poolSize, testResults)));

            for (var round = 0; round < messagesToSend; ++round)
            {
                testRouter.Actor.DoSomeMath(round, round, round);
            }

            Assert.Equal(totalMessagesExpected, testResults.GetReceivedCount());

            var routerActor = (MathCommandRouter)testRouter.ActorInside;
            foreach (var routee in routerActor.Routees)
            {
                Assert.Equal(messagesToSend, routee.MessageCount);
            }
        }

        private class MathCommandRouter : BroadcastRouter<IThreeArgConsumerProtocol>, IThreeArgConsumerProtocol
        {
            public MathCommandRouter(int poolSize, TestResults testResults) :
                base(new RouterSpecification<IThreeArgConsumerProtocol>(
                    poolSize,
                    Definition.Has<MathCommandWorker>(Definition.Parameters(testResults))))
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
            private readonly TestResults _testResults;

            public MathCommandWorker(TestResults testResults)
            {
                _testResults = testResults;
            }

            public void DoSomeMath(int arg1, int arg2, int arg3)
            {
                var sum = arg1 + arg2 + arg3;
                var product = arg1 * arg2 * arg3;
                _testResults.Received.WriteUsing("receivedCount", 1);
            }
        }

        private class TestResults
        {
            private readonly AtomicInteger _receivedCount = new AtomicInteger(0);
            internal readonly AccessSafely Received;

            public TestResults(AccessSafely received) => Received = received;

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.Received.WritingWith<int>("receivedCount", _ => testResults._receivedCount.IncrementAndGet());
                testResults.Received.ReadingWith("receivedCount", testResults._receivedCount.Get);
                return testResults;
            }

            public int GetReceivedCount() => Received.ReadFrom<int>("receivedCount");
        }
    }

    public interface IThreeArgConsumerProtocol
    {
        void DoSomeMath(int arg1, int arg2, int arg3);
    }
}
