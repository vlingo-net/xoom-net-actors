// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class AddressTest : IDisposable
    {
        private readonly World world;
        public AddressTest()
        {
            world = World.Start("test");
        }
        public void Dispose()
        {
            world.Terminate();
        }

        [Fact]
        public void TestNameGiven()
        {
            var address = world.AddressFactory.UniqueWith("test-address");
            var id = world.AddressFactory.TestNextIdValue() - 1;

            Assert.NotNull(address);
            Assert.Equal(id, address.Id);
            Assert.Equal("test-address", address.Name);

            var another = world.AddressFactory.UniqueWith("another-address");

            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(id, address.GetHashCode());
        }

        [Fact]
        public void TestNameAndIdGiven()
        {
            const int id = 123;

            var address = world.AddressFactory.From(id, "test-address");

            Assert.NotNull(address);
            Assert.Equal(123, address.Id);
            Assert.Equal("test-address", address.Name);

            var another = world.AddressFactory.From(456, "test-address");

            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(address, world.AddressFactory.From(123, "test-address"));
            Assert.Equal(0, address.CompareTo(world.AddressFactory.From(123, "test-address")));
            Assert.Equal(id, address.GetHashCode());
        }
    }
}
