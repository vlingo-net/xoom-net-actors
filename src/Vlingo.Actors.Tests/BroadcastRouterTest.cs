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
                base(new RouterSpecification(
                    poolSize,
                    Definition.Has<MathCommandWorker>(Definition.Parameters(testResults)),
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
            private readonly TestResults testResults;

            public MathCommandWorker(TestResults testResults)
            {
                this.testResults = testResults;
            }

            public void DoSomeMath(int arg1, int arg2, int arg3)
            {
                var sum = arg1 + arg2 + arg3;
                var product = arg1 * arg2 * arg3;
                testResults.received.WriteUsing("receivedCount", 1);
            }
        }

        private class TestResults
        {
            private readonly AtomicInteger receivedCount = new AtomicInteger(0);
            internal readonly AccessSafely received;

            public TestResults(AccessSafely received)
            {
                this.received = received;
            }

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.received.WritingWith<int>("receivedCount", _ => testResults.receivedCount.IncrementAndGet());
                testResults.received.ReadingWith("receivedCount", testResults.receivedCount.Get);
                return testResults;
            }

            public int GetReceivedCount() => received.ReadFrom<int>("receivedCount");
        }
    }

    public interface IThreeArgConsumerProtocol
    {
        void DoSomeMath(int arg1, int arg2, int arg3);
    }
}
