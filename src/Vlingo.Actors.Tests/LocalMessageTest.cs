// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class LocalMessageTest : ActorsTest
    {
        [Fact]
        public void TestDeliverHappy()
        {
            var testResults = new SimpleTestResults();
            TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));
            Action<ISimple> consumer = x => x.Simple();
            var message = new LocalMessage<ISimple>(SimpleActor.Instance.Value, consumer, "Simple()");

            message.Deliver();
            testResults.UntilSimple.Completes();

            Assert.Equal(1, testResults.Deliveries.Get());
        }

        [Fact]
        public void TestDeliverStopped()
        {
            var testResults = new SimpleTestResults();
            TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));
            testResults.UntilSimple = TestUntil.Happenings(1);

            SimpleActor.Instance.Value.Stop();

            Action<ISimple> consumer = x => x.Simple();
            var message = new LocalMessage<ISimple>(SimpleActor.Instance.Value, consumer, "Simple()");

            message.Deliver();

            Assert.Equal(1, testResults.UntilSimple.Remaining);
            Assert.Equal(0, testResults.Deliveries.Get());
        }

        [Fact]
        public void TestDeliverWithParameters()
        {
            var testResults = new SimpleTestResults();
            TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));
            testResults.UntilSimple = TestUntil.Happenings(1);

            Action<ISimple> consumer = x => x.Simple2(2);
            var message = new LocalMessage<ISimple>(SimpleActor.Instance.Value, consumer, "Simple2(int)");

            message.Deliver();
            testResults.UntilSimple.Completes();

            Assert.Equal(1, testResults.Deliveries.Get());
        }

        private class SimpleTestResults
        {
            public AtomicInteger Deliveries { get; set; } = new AtomicInteger(0);
            public TestUntil UntilSimple { get; set; } = TestUntil.Happenings(0);
        }

        private class SimpleActor : Actor, ISimple
        {
            public static readonly ThreadLocal<SimpleActor> Instance = new ThreadLocal<SimpleActor>();

            private readonly SimpleTestResults testResults;

            public SimpleActor(SimpleTestResults testResults)
            {
                this.testResults = testResults;
                Instance.Value = this;
            }

            public void Simple()
            {
                testResults.Deliveries.IncrementAndGet();
                testResults.UntilSimple.Happened();
            }

            public void Simple2(int val)
            {
                testResults.Deliveries.IncrementAndGet();
                testResults.UntilSimple.Happened();
            }
        }
    }

    public interface ISimple
    {
        void Simple();
        void Simple2(int val);
    }
}
