// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ActorFactoryTest : ActorsTest
    {
        [Fact]
        public void TestActorForWithNoParametersAndDefaults()
        {
            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters);

            var address = World.AddressFactory.UniqueWith("test-actor");

            var mailbox = new TestMailbox();

            var actor =
            ActorFactory.ActorFor(
                    World.Stage,
                    World.DefaultParent,
                    definition,
                    address,
                    mailbox,
                    null,
                    World.DefaultLogger);

            Assert.NotNull(actor);
            Assert.NotNull(actor.Stage);
            Assert.Equal(World.Stage, actor.Stage);
            Assert.NotNull(actor.LifeCycle.Environment.Parent);
            Assert.Equal(World.DefaultParent, actor.LifeCycle.Environment.Parent);
            Assert.NotNull(actor.LifeCycle.Environment);
            Assert.NotNull(actor.LifeCycle.Environment.Definition);
            Assert.Equal(definition, actor.LifeCycle.Environment.Definition);
            Assert.NotNull(actor.Address);
            Assert.Equal(address, actor.Address);
            Assert.NotNull(actor.LifeCycle.Environment.Mailbox);
            Assert.Equal(mailbox, actor.LifeCycle.Environment.Mailbox);
        }

        [Fact]
        public void TestActorForWithParameters()
        {
            World.ActorFor<IParentInterface>(Definition.Has<ParentInterfaceActor>(Definition.NoParameters));

            var actorName = "test-child";

            var definition =
                Definition.Has<TestInterfaceWithParamsActor>(
                        Definition.Parameters("test-text", 100),
                        ParentInterfaceActor.Instance.Value,
                        actorName);


            var address = World.AddressFactory.UniqueWith(actorName);

            var mailbox = new TestMailbox();

            var actor =
                ActorFactory.ActorFor(
                        World.Stage,
                        definition.Parent,
                        definition,
                        address,
                        mailbox,
                        null,
                        World.DefaultLogger);

            Assert.NotNull(actor);
            Assert.NotNull(actor.Stage);
            Assert.Equal(World.Stage, actor.Stage);
            Assert.NotNull(actor.LifeCycle.Environment.Parent);
            Assert.Equal(ParentInterfaceActor.Instance.Value, actor.LifeCycle.Environment.Parent);
            Assert.NotNull(actor.LifeCycle.Environment);
            Assert.NotNull(actor.LifeCycle.Environment.Definition);
            Assert.Equal(definition, actor.LifeCycle.Environment.Definition);
            Assert.NotNull(actor.LifeCycle.Environment.Address);
            Assert.Equal(address, actor.LifeCycle.Environment.Address);
            Assert.NotNull(actor.LifeCycle.Environment.Mailbox);
            Assert.Equal(mailbox, actor.LifeCycle.Environment.Mailbox);
        }

        [Fact]
        public void TestConstructorFailure()
        {
            World.ActorFor<IParentInterface>(Definition.Has<ParentInterfaceActor>(Definition.NoParameters));
            var address = World.AddressFactory.UniqueWith("test-actor-ctor-failure");
            var definition = Definition.Has<FailureActor>(
                Definition.Parameters("test-ctor-failure", -100), 
                ParentInterfaceActor.Instance.Value, 
                address.Name);
            var mailbox = new TestMailbox();

            Assert.Throws<ArgumentException>(() => ActorFactory.ActorFor(
                World.Stage,
                definition.Parent,
                definition,
                address,
                mailbox,
                null,
                World.DefaultLogger));
        }

        private class ParentInterfaceActor : Actor, IParentInterface
        {
            public static ThreadLocal<ParentInterfaceActor> Instance { get; } = new ThreadLocal<ParentInterfaceActor>();

            public ParentInterfaceActor()
            {
                Instance.Value = this;
            }
        }

        public interface ITestInterface { }

        private class TestInterfaceActor : Actor, ITestInterface { }

        private class TestInterfaceWithParamsActor : Actor, ITestInterface
        {
            public TestInterfaceWithParamsActor(string text, int val) { }
        }

        private class FailureActor : Actor, ITestInterface
        {
            public FailureActor(string text, int val)
            {
                throw new InvalidOperationException("Failed in ctor with: " + text + " and: " + val);
            }
        }
    }
}