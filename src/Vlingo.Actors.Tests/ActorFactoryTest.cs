// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors;
using Vlingo.Actors.Plugin.Mailbox.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorFactoryTest : IDisposable
    {
        private World world;

        public ActorFactoryTest() => world = World.Start("test-world");

        public void Dispose() => world.Terminate();

        [Fact]
        public void TestActorForWithNoParametersAndDefaults()
        {
            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters);

            var address = world.AddressFactory.AddressFrom("test-actor");

            var mailbox = new TestMailbox();

            var actor =
            ActorFactory.ActorFor(
                    world.Stage,
                    world.DefaultParent,
                    definition,
                    address,
                    mailbox,
                    null,
                    world.DefaultLogger);

            Assert.NotNull(actor);
            Assert.NotNull(actor.Stage);
            Assert.Equal(world.Stage, actor.Stage);
            Assert.NotNull(actor.LifeCycle.Environment.Parent);
            Assert.Equal(world.DefaultParent, actor.LifeCycle.Environment.Parent);
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
            world.ActorFor<IParentInterface>(Definition.Has<ParentInterfaceActor>(Definition.NoParameters));

            var actorName = "test-child";

            var definition =
                Definition.Has<TestInterfaceWithParamsActor>(
                        Definition.Parameters("test-text", 100),
                        ParentInterfaceActor.Instance.Value,
                        actorName);


            var address = world.AddressFactory.AddressFrom(actorName);

            var mailbox = new TestMailbox();

            var actor =
                ActorFactory.ActorFor(
                        world.Stage,
                        definition.Parent,
                        definition,
                        address,
                        mailbox,
                        null,
                        world.DefaultLogger);

            Assert.NotNull(actor);
            Assert.NotNull(actor.Stage);
            Assert.Equal(world.Stage, actor.Stage);
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
    }

    public interface IParentInterface { }

    public sealed class ParentInterfaceActor : Actor, IParentInterface
    {
        public static ThreadLocal<ParentInterfaceActor> Instance { get; } = new ThreadLocal<ParentInterfaceActor>();

        public ParentInterfaceActor()
        {
            Instance.Value = this;
        }
    }

    public interface ITestInterface { }

    internal sealed class TestInterfaceActor : Actor, ITestInterface { }

    internal sealed class TestInterfaceWithParamsActor : Actor, ITestInterface
    {
        public TestInterfaceWithParamsActor(string text, int val) { }
    }
}