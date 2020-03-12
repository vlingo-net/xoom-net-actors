// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorStopTest : ActorsTest
    {
        [Fact]
        public void TestStopActors()
        {
            var results = new TestResults();
            var beforeStartCountAccess = results.BeforeStartCountAccessCompletes(12);
            World.DefaultLogger.Debug("Test: TestStopActors: starting actors");

            var stoppables = SetUpActors(World, results);
            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].CreateChildren();
            }

            var beforeStartCount = beforeStartCountAccess.ReadFrom<int>("value");
            Assert.Equal(12, beforeStartCount);

            World.DefaultLogger.Debug("Test: TestStopActors: stopping actors");

            results.TerminatingAccessCompletes(0).WriteUsing("value", false);

            var stopCountAccess = results.StopCountAccessCompletes(12);
            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].Stop();
            }

            var stopCount = stopCountAccess.ReadFromExpecting("value", 12, 10);
            Assert.Equal(12, stopCount);

            World.DefaultLogger.Debug("Test: TestStopActors: stopped actors");
            World.DefaultLogger.Debug("Test: TestStopActors: terminating world");

            var terminatingStopCountAccess = results.TerminatingStopCountAccessCompletes(0);
            results.TerminatingAccessCompletes(0).WriteUsing("value", true);
            World.Terminate();

            var terminatingStopCount = terminatingStopCountAccess.ReadFrom<int>("value");
            Assert.Equal(0, terminatingStopCount);
        }

        [Fact]
        public void TestWorldTerminateToStopAllActors()
        {
            var results = new TestResults();
            var beforeStartCountAccess = results.BeforeStartCountAccessCompletes(12);

            var stoppables = SetUpActors(World, results);

            for (int idx = 0; idx < stoppables.Length; ++idx)
            {
                stoppables[idx].CreateChildren();
            }

            beforeStartCountAccess.ReadFrom<int>("value");
            var terminatingStopAccess = results.TerminatingStopCountAccessCompletes(12);
            results.TerminatingAccessCompletes(0).WriteUsing("value", true);
            World.Terminate();
            
            var terminatingStopCount = terminatingStopAccess.ReadFrom<int>("value");
            Assert.Equal(12, terminatingStopCount);
        }

        private IChildCreatingStoppable[] SetUpActors(World world, TestResults results)
        {
            var stoppables = new IChildCreatingStoppable[3];
            stoppables[0] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), "p1"));
            stoppables[1] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), "p2"));
            stoppables[2] = world.ActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), "p3"));
            return stoppables;
        }

        private class ChildCreatingStoppableActor : Actor, IChildCreatingStoppable
        {
            private readonly TestResults results;

            public ChildCreatingStoppableActor(TestResults results)
            {
                this.results = results;
            }

            public void CreateChildren()
            {
                var pre = Address.Name;
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), $"{pre}.1"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), $"{pre}.2"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(results), $"{pre}.3"));
            }

            protected internal override void BeforeStart()
            {
                base.BeforeStart();
                results.beforeStartCountAccess.WriteUsing("value", 1);
            }

            protected internal override void AfterStop()
            {
                if (results.terminatingAccess.ReadFromNow<bool>("value"))
                {
                    results.terminatingStopCountAccess.WriteUsing("value", 1);
                }
                else
                {
                    results.stopCountAccess.WriteUsing("value", 1);
                }
            }
        }

        private class TestResults
        {
            internal AccessSafely beforeStartCountAccess = AccessSafely.AfterCompleting(1);
            internal AtomicInteger beforeStartCount = new AtomicInteger(0);

            internal AccessSafely stopCountAccess = AccessSafely.AfterCompleting(1);
            internal AtomicInteger stopCount = new AtomicInteger(0);

            internal AccessSafely terminatingAccess = AccessSafely.AfterCompleting(1);
            internal AtomicBoolean terminating = new AtomicBoolean(false);

            internal AccessSafely terminatingStopCountAccess = AccessSafely.AfterCompleting(1);
            internal AtomicInteger terminatingStopCount = new AtomicInteger(0);

            public AccessSafely BeforeStartCountAccessCompletes(int times)
            {
                beforeStartCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => beforeStartCount.Set(beforeStartCount.Get() + value))
                    .ReadingWith("value", () => beforeStartCount.Get());

                return beforeStartCountAccess;
            }

            public AccessSafely StopCountAccessCompletes(int times)
            {
                stopCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => stopCount.Set(stopCount.Get() + value))
                    .ReadingWith("value", () => stopCount.Get());

                return stopCountAccess;
            }

            public AccessSafely TerminatingAccessCompletes(int times)
            {
                terminatingAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (bool flag) => terminating.Set(flag))
                    .ReadingWith("value", () => terminating.Get());

                return terminatingAccess;
            }

            public AccessSafely TerminatingStopCountAccessCompletes(int times)
            {
                terminatingStopCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => terminatingStopCount.Set(terminatingStopCount.Get() + value))
                    .ReadingWith("value", () => terminatingStopCount.Get());

                return terminatingStopCountAccess;
            }
        }
    }

    public interface IChildCreatingStoppable : IStoppable
    {
        void CreateChildren();
    }
}
