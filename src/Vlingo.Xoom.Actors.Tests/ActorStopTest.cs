// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
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
            private readonly TestResults _results;

            public ChildCreatingStoppableActor(TestResults results) => _results = results;

            public void CreateChildren()
            {
                var pre = Address.Name;
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(_results), $"{pre}.1"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(_results), $"{pre}.2"));
                ChildActorFor<IChildCreatingStoppable>(Definition.Has<ChildCreatingStoppableActor>(Definition.Parameters(_results), $"{pre}.3"));
            }

            protected internal override void BeforeStart()
            {
                base.BeforeStart();
                _results.BeforeStartCountAccess.WriteUsing("value", 1);
            }

            protected internal override void AfterStop()
            {
                if (_results.TerminatingAccess.ReadFromNow<bool>("value"))
                {
                    _results.TerminatingStopCountAccess.WriteUsing("value", 1);
                }
                else
                {
                    _results.StopCountAccess.WriteUsing("value", 1);
                }
            }
        }

        private class TestResults
        {
            internal AccessSafely BeforeStartCountAccess = AccessSafely.AfterCompleting(1);
            internal readonly AtomicInteger BeforeStartCount = new AtomicInteger(0);

            internal AccessSafely StopCountAccess = AccessSafely.AfterCompleting(1);
            internal readonly AtomicInteger StopCount = new AtomicInteger(0);

            internal AccessSafely TerminatingAccess = AccessSafely.AfterCompleting(1);
            internal readonly AtomicBoolean Terminating = new AtomicBoolean(false);

            internal AccessSafely TerminatingStopCountAccess = AccessSafely.AfterCompleting(1);
            internal readonly AtomicInteger TerminatingStopCount = new AtomicInteger(0);

            public AccessSafely BeforeStartCountAccessCompletes(int times)
            {
                BeforeStartCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => BeforeStartCount.Set(BeforeStartCount.Get() + value))
                    .ReadingWith("value", () => BeforeStartCount.Get());

                return BeforeStartCountAccess;
            }

            public AccessSafely StopCountAccessCompletes(int times)
            {
                StopCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => StopCount.Set(StopCount.Get() + value))
                    .ReadingWith("value", () => StopCount.Get());

                return StopCountAccess;
            }

            public AccessSafely TerminatingAccessCompletes(int times)
            {
                TerminatingAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (bool flag) => Terminating.Set(flag))
                    .ReadingWith("value", () => Terminating.Get());

                return TerminatingAccess;
            }

            public AccessSafely TerminatingStopCountAccessCompletes(int times)
            {
                TerminatingStopCountAccess = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("value", (int value) => TerminatingStopCount.Set(TerminatingStopCount.Get() + value))
                    .ReadingWith("value", () => TerminatingStopCount.Get());

                return TerminatingStopCountAccess;
            }
        }
    }

    public interface IChildCreatingStoppable : IStoppable
    {
        void CreateChildren();
    }
}
