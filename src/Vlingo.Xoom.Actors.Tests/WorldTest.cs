// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using NSubstitute;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class WorldTest : ActorsTest
    {
        [Fact]
        public void TestStartWorld()
        {
            Assert.NotNull(World.DeadLetters);
            Assert.Equal($"{typeof(WorldTest).Name}-world", World.Name);
            Assert.NotNull(World.Stage);
            Assert.NotNull(World.Stage.Scheduler);
            Assert.Equal(World, World.Stage.World);
            Assert.False(World.IsTerminated);
            Assert.NotNull(World.FindDefaultMailboxName());
            Assert.Equal("queueMailbox", World.FindDefaultMailboxName());
            Assert.NotNull(World.AssignMailbox("queueMailbox", 10));
            Assert.NotNull(World.DefaultParent);
            Assert.NotNull(World.PrivateRoot);
            Assert.NotNull(World.PublicRoot);
        }

        [Fact]
        public void TestWorldActorForDefintion()
        {
            var testResults = new TestResults(1);
            var simple = World.ActorFor<ISimpleWorld>(() => new SimpleActor(testResults));

            simple.SimpleSay();

            Assert.True(testResults.Invoked);
        }

        [Fact]
        public void TestWorldActorForFlat()
        {
            var testResults = new TestResults(1);
            var simple = World.ActorFor<ISimpleWorld>(Definition.Has(() => new SimpleActor(testResults)));

            simple.SimpleSay();

            Assert.True(testResults.Invoked);
        }

        [Fact]
        public void TestWorldNoDefintionActorFor()
        {
            var testResults = new TestResults(1);
            var simple = World.ActorFor<ISimpleWorld>(typeof(SimpleActor), testResults);

            simple.SimpleSay();

            Assert.True(testResults.Invoked);
        }

        [Fact]
        public void TestThatARegisteredDependencyCanBeResolved()
        {
            var name = Guid.NewGuid().ToString();
            var dep = Substitute.For<IAnyDependecy>();
            World.RegisterDynamic(name, dep);

            var result = World.ResolveDynamic<IAnyDependecy>(name);
            Assert.Same(dep, result);
        }

        [Fact]
        public void TestThatResolvesAMissingDependencyReturnsNull()
        {
            var name = Guid.NewGuid().ToString();
            var result = World.ResolveDynamic<IAnyDependecy>(name);
            Assert.Null(result);
        }

        [Fact(DisplayName = "TestTermination")]
        public override void Dispose()
        {
            base.Dispose();
            Assert.True(World.Stage.IsStopped);
            Assert.True(World.IsTerminated);
        }

        internal class SimpleActor : Actor, ISimpleWorld
        {
            private readonly TestResults _testResults;

            public SimpleActor(TestResults testResults)
            {
                _testResults = testResults;
            }

            public void SimpleSay() => _testResults.SetInvoked(true);
        }

        internal class TestResults
        {
            private readonly AccessSafely _safely;
            
            public TestResults(int times)
            {
                var invoked = new AtomicBoolean(false);
                _safely = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith<bool>("invoked", val => invoked.Set(val))
                    .ReadingWith("invoked", invoked.Get);
            }

            public bool Invoked => _safely.ReadFrom<bool>("invoked");

            public void SetInvoked(bool invoked) => _safely.WriteUsing("invoked", invoked);
        }

        public interface IAnyDependecy { }
    }

    public interface ISimpleWorld
    {
        void SimpleSay();
    }
}
