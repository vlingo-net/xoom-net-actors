// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorStopTest : ActorsTest
    {
        [Fact]
        public void TestWorldTerminateToStopAllActors()
        {
            var testSpecs = new TestResults();
            testSpecs.untilStart = TestUntil.Happenings(12);

            var stoppables = SetUpActors(World, testSpecs);

            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].CreateChildren();
            }

            testSpecs.untilStart.CompletesWithin(2000);
            testSpecs.untilTerminatingStop = TestUntil.Happenings(12);
            testSpecs.terminating.Set(true);

            World.Terminate();

            testSpecs.untilTerminatingStop.CompletesWithin(2000);

            Assert.Equal(12, testSpecs.terminatingStopCount.Get());
        }

        [Fact]
        public void TestStopActors()
        {
            var testResults = new TestResults();
            testResults.untilStart = TestUntil.Happenings(12);

            var stoppables = SetUpActors(World, testResults);
            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].CreateChildren();
            }

            testResults.untilStart.CompletesWithin(2000);

            testResults.untilStop = TestUntil.Happenings(12);

            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].Stop();
            }

            testResults.untilStop.CompletesWithin(2000);

            Assert.Equal(12, testResults.stopCount.Get());

            testResults.untilTerminatingStop = TestUntil.Happenings(0);

            testResults.terminating.Set(true);
            World.Terminate();

            testResults.untilTerminatingStop.CompletesWithin(2000);

            Assert.Equal(0, testResults.terminatingStopCount.Get());
        }

        private IChildCreatingStoppable[] SetUpActors(World world, TestResults testResults)
        {
            var stoppables = new IChildCreatingStoppable[3];
            stoppables[0] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), "p1"));
            stoppables[1] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), "p2"));
            stoppables[2] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), "p3"));
            return stoppables;
        }

        private class ChildCreatingStoppableActor : Actor, IChildCreatingStoppable
        {
            private readonly TestResults testResults;

            public ChildCreatingStoppableActor(TestResults testSpecs)
            {
                testResults = testSpecs;
            }

            public void CreateChildren()
            {
                var pre = Address.Name;
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), $"{pre}.1"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), $"{pre}.2"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(testResults), $"{pre}.3"));
            }

            protected internal override void BeforeStart()
            {
                base.BeforeStart();
                if (testResults.untilStart != null)
                {
                    testResults.untilStart.Happened();
                }
            }

            private static readonly object afterStopMutex = new object();
            protected internal override void AfterStop()
            {
                lock (afterStopMutex)
                {
                    if (testResults.terminating.Get())
                    {
                        var count = testResults.terminatingStopCount.IncrementAndGet();
                        Console.WriteLine("TERMINATING AND STOPPED: " + count + " ");
                        testResults.untilTerminatingStop.Happened();
                    }
                    else
                    {
                        var count = testResults.stopCount.IncrementAndGet();
                        Console.WriteLine("STOPPED: " + count + " ");
                        //testResults.stopCount.incrementAndGet();
                        testResults.untilStop.Happened();
                    }
                }
            }
        }

        private class TestResults
        {
            public AtomicInteger stopCount = new AtomicInteger(0);
            public AtomicBoolean terminating = new AtomicBoolean(false);
            public AtomicInteger terminatingStopCount = new AtomicInteger(0);
            public TestUntil untilStart = TestUntil.Happenings(0);
            public TestUntil untilStop = TestUntil.Happenings(0);
            public TestUntil untilTerminatingStop = TestUntil.Happenings(0);
        }
    }

    public interface IChildCreatingStoppable : IStoppable
    {
        void CreateChildren();
    }
}
