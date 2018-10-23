// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorEnvironmentTest : ActorsTest
    {
        [Fact]
        public void TestExpectedEnvironment()
        {
            var definition = Definition.Has<EnvironmentProviderActor>(Definition.NoParameters, "test-env");
            var env = TestWorld.ActorFor<IEnvironmentProvider>(definition);
            var state = env.ViewTestState();
            var actorDefinition = state.ValueOf<Definition>("definition");

            Assert.Empty(TestWorld.AllMessagesFor(env.Address));
            Assert.Equal(TestWorld.World.AddressFactory.TestNextIdValue() - 1, state.ValueOf<Address>("address").Id);
            Assert.Equal(definition.ActorName, actorDefinition.ActorName);
            Assert.Equal(definition.Parameters(), actorDefinition.Parameters());
            Assert.Equal(TestWorld.World.DefaultParent, state.ValueOf<Actor>("parent"));
            Assert.Same(TestWorld.Stage, state.ValueOf<Stage>("stage"));
        }

        [Fact]
        public void TestSecuredEnvironment()
        {
            var definition = Definition.Has<CannotProvideEnvironmentActor>(Definition.NoParameters, "test-env");
            var env = TestWorld.ActorFor<IEnvironmentProvider>(definition);
            var state = env.ViewTestState();

            Assert.Empty(TestWorld.AllMessagesFor(env.Address));
            Assert.NotNull(state.ValueOf<object>("address"));
            Assert.Null(state.ValueOf<object>("definition"));
            Assert.Null(state.ValueOf<object>("parent"));
            Assert.Null(state.ValueOf<object>("stage"));
        }

        [Fact]
        public void TestStop()
        {
            var definition = Definition.Has<StopTesterActor>(Definition.Parameters(0), "test-stop");
            var stopTest = TestWorld.ActorFor<IStopTester>(definition);
            var env = stopTest.ViewTestState().ValueOf<Environment>("env");

            Assert.Single(env.Children);
            Assert.False(env.IsStopped);
            Assert.False(env.Mailbox.IsClosed);

            stopTest.Actor.Stop();

            Assert.Empty(env.Children);
            Assert.True(env.IsStopped);
            Assert.True(env.Mailbox.IsClosed);
        }
    }

    public interface IEnvironmentProvider { }

    public class EnvironmentProviderActor : Actor, IEnvironmentProvider
    {

        public EnvironmentProviderActor() { }

        public override TestState ViewTestState()
        {
            return new TestState()
                    .PutValue("address", Address)
                    .PutValue("definition", Definition)
                    .PutValue("parent", Parent)
                    .PutValue("stage", Stage);
        }
    }

    public class CannotProvideEnvironmentActor : Actor, IEnvironmentProvider
    {
        public CannotProvideEnvironmentActor()
        {
            Secure();
        }

        public override TestState ViewTestState()
        {
            var state = new TestState();

            try
            {
                state.PutValue("address", base.Address);
            }
            catch { }

            try
            {
                state.PutValue("definition", base.Definition);
            }
            catch { }

            try
            {
                state.PutValue("parent", base.Parent);
            }
            catch { }

            try
            {
                state.PutValue("stage", base.Stage);
            }
            catch { }

            return state;
        }
    }

    public interface IStopTester : IStoppable { }

    public class StopTesterActor : Actor, IStopTester
    {
        public StopTesterActor(int count)
        {
            if (count == 0)
            {
                ChildActorFor<IStopTester>(Definition.Has<StopTesterActor>(Definition.Parameters(1), "test-stop-1"));
            }
        }

        public override TestState ViewTestState()
            => new TestState().PutValue("env", LifeCycle.Environment);
    }
}
