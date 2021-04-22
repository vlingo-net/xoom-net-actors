// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;
using static Vlingo.Actors.Tests.ActorFactoryTest;

namespace Vlingo.Actors.Tests
{
    public class DirectoryTest : ActorsTest
    {
        [Fact]
        public void TestDirectoryRegister()
        {
            var directory = new Directory(new BasicAddress(0, ""), 32, 32);
            var address = World.AddressFactory.UniqueWith("test-actor");
            var actor = new TestInterfaceActor();

            directory.Register(address, actor);

            Assert.True(directory.IsRegistered(address));
            Assert.False(directory.IsRegistered(World.AddressFactory.UniqueWith("another-actor")));
        }

        [Fact]
        public void TestDirectoryRemove()
        {
            var directory = new Directory(new BasicAddress(0, ""), 32, 32);
            var address = World.AddressFactory.UniqueWith("test-actor");
            var actor = new TestInterfaceActor();

            directory.Register(address, actor);

            Assert.True(directory.IsRegistered(address));

            directory.Remove(address);

            Assert.False(directory.IsRegistered(address));
        }

        [Fact]
        public void TestDirectoryAlreadyRegistered()
        {
            var directory = new Directory(new BasicAddress(0, ""), 32, 32);
            var address = World.AddressFactory.UniqueWith("test-actor");
            var actor = new TestInterfaceActor();

            directory.Register(address, actor);

            Assert.Throws<ActorAddressAlreadyRegisteredException>(() => directory.Register(address, new TestInterfaceActor()));
        }

        [Fact]
        public void TestDirectoryFindsRegistered()
        {
            var directory = new Directory(new BasicAddress(0, ""), 32, 32);
            var address1 = World.AddressFactory.UniqueWith("test-actor1");
            var address2 = World.AddressFactory.UniqueWith("test-actor2");
            var address3 = World.AddressFactory.UniqueWith("test-actor3");
            var address4 = World.AddressFactory.UniqueWith("test-actor4");
            var address5 = World.AddressFactory.UniqueWith("test-actor5");

            directory.Register(address1, new TestInterfaceActor());
            directory.Register(address2, new TestInterfaceActor());
            directory.Register(address3, new TestInterfaceActor());
            directory.Register(address4, new TestInterfaceActor());
            directory.Register(address5, new TestInterfaceActor());

            Assert.NotNull(directory.ActorOf(address1));
            Assert.NotNull(directory.ActorOf(address2));
            Assert.NotNull(directory.ActorOf(address3));
            Assert.NotNull(directory.ActorOf(address4));
            Assert.NotNull(directory.ActorOf(address5));
            Assert.Null(directory.ActorOf(World.AddressFactory.UniqueWith("test-actor6")));
        }

        private class TestInterfaceActor : Actor, ITestInterface { }
    }
}
