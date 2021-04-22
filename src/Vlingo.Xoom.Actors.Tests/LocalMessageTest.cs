// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class LocalMessageTest : ActorsTest
    {
        [Fact]
        public void TestDeliverHappy()
        {
            var testResults = new SimpleTestResults(1);
            var testActor = TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));
            Action<ISimple> consumer = x => x.Simple();
            var message = new LocalMessage<ISimple>(testActor.ActorInside, consumer, "Simple()");

            message.Deliver();

            Assert.Equal(1, testResults.GetDeliveries());
        }

        [Fact]
        public void TestDeliverStopped()
        {
            var testResults = new SimpleTestResults(0);
            var testActor = TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));

            SimpleActor.Instance.Value.Stop();

            Action<ISimple> consumer = x => x.Simple();
            var message = new LocalMessage<ISimple>(testActor.ActorInside, consumer, "Simple()");

            message.Deliver();

            Assert.Equal(0, testResults.GetDeliveries());
        }

        [Fact]
        public void TestDeliverWithParameters()
        {
            var testResults = new SimpleTestResults(1);
            var testActor = TestWorld.ActorFor<ISimple>(Definition.Has<SimpleActor>(Definition.Parameters(testResults), "test1-actor"));

            Action<ISimple> consumer = x => x.Simple2(2);
            var message = new LocalMessage<ISimple>(testActor.ActorInside, consumer, "Simple2(int)");

            message.Deliver();

            Assert.Equal(1, testResults.GetDeliveries());
        }

        private class SimpleTestResults
        {
            private readonly AccessSafely _deliveries;

            public SimpleTestResults(int times)
            {
                var count = new AtomicInteger(0);
                _deliveries = AccessSafely.AfterCompleting(times);
                _deliveries.WritingWith<int>("deliveries", _ => count.IncrementAndGet());
                _deliveries.ReadingWith("deliveries", count.Get);
            }

            internal void Increment() => _deliveries.WriteUsing("deliveries", 1);

            internal int GetDeliveries() => _deliveries.ReadFrom<int>("deliveries");
        }

        private class SimpleActor : Actor, ISimple
        {
            public static readonly ThreadLocal<SimpleActor> Instance = new ThreadLocal<SimpleActor>();

            private readonly SimpleTestResults _testResults;

            public SimpleActor(SimpleTestResults testResults)
            {
                _testResults = testResults;
                Instance.Value = this;
            }

            public void Simple() => _testResults.Increment();

            public void Simple2(int val) => _testResults.Increment();
        }
    }

    public interface ISimple
    {
        void Simple();
        void Simple2(int val);
    }
}
