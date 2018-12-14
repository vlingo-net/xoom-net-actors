// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class StageTest : ActorsTest
    {
        [Fact]
        public void TestActorForDefinitionAndProtocol()
        {
            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters);
            var test = World.ActorFor<INoProtocol>(definition);

            Assert.NotNull(test);
            Assert.NotNull(TestInterfaceActor.Instance.Value);
            Assert.Equal(World.DefaultParent, TestInterfaceActor.Instance.Value.LifeCycle.Environment.Parent);
        }

        [Fact]
        public void TestActorForAll()
        {
            World.ActorFor<INoProtocol>(Definition.Has<ParentInterfaceActor>(Definition.NoParameters));
            var definition = Definition.Has<TestInterfaceActor>(
                Definition.NoParameters,
                ParentInterfaceActor.Instance.Value,
                TestMailbox.Name,
                "test-actor");
            var test = World.Stage.ActorFor<INoProtocol>(definition);

            Assert.NotNull(test);
            Assert.NotNull(TestInterfaceActor.Instance.Value);
        }

        [Fact]
        public void TestDirectoryScan()
        {
            var scanFound = new AtomicInteger(0);

            var address1 = World.AddressFactory.UniqueWith("test-actor1");
            var address2 = World.AddressFactory.UniqueWith("test-actor2");
            var address3 = World.AddressFactory.UniqueWith("test-actor3");
            var address4 = World.AddressFactory.UniqueWith("test-actor4");
            var address5 = World.AddressFactory.UniqueWith("test-actor5");
            var address6 = World.AddressFactory.UniqueWith("test-actor6");
            var address7 = World.AddressFactory.UniqueWith("test-actor7");

            World.Stage.Directory.Register(address1, new TestInterfaceActor());
            World.Stage.Directory.Register(address2, new TestInterfaceActor());
            World.Stage.Directory.Register(address3, new TestInterfaceActor());
            World.Stage.Directory.Register(address4, new TestInterfaceActor());
            World.Stage.Directory.Register(address5, new TestInterfaceActor());

            var until = Until(7);
            Action<INoProtocol> afterConsumer = actor =>
            {
                Assert.NotNull(actor);
                scanFound.IncrementAndGet();
                until.Happened();
            };

            World.Stage.ActorOf<INoProtocol>(address5).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address4).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address3).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address2).AndThenConsume(afterConsumer);
            World.Stage.ActorOf<INoProtocol>(address1).AndThenConsume(afterConsumer);

            World.Stage.ActorOf<INoProtocol>(address6)
                .AndThenConsume(actor =>
                {
                    Assert.Null(actor);
                    until.Happened();
                })
                .Otherwise(actor =>
                {
                    Assert.Null(actor);
                    until.Happened();
                    return null;
                });
            World.Stage.ActorOf<INoProtocol>(address7)
                .AndThenConsume(actor =>
                {
                    Assert.Null(actor);
                    until.Happened();
                })
                .Otherwise(actor =>
                {
                    Assert.Null(actor);
                    until.Happened();
                    return null;
                });

            until.Completes();
            Assert.Equal(5, scanFound.Get());
        }

        private class ParentInterfaceActor : Actor, INoProtocol
        {
            public static ThreadLocal<ParentInterfaceActor> Instance = new ThreadLocal<ParentInterfaceActor>();
            public ParentInterfaceActor() => Instance.Value = this;
        }

        private class TestInterfaceActor : Actor, INoProtocol
        {
            public static ThreadLocal<TestInterfaceActor> Instance = new ThreadLocal<TestInterfaceActor>();
            public TestInterfaceActor() => Instance.Value = this;
        }
    }
}
