// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class AddressTest : ActorsTest
    {
        [Fact]
        public void TestNameGiven()
        {
            var address = World.AddressFactory.UniqueWith("test-address");
            var id = World.AddressFactory.TestNextIdValue() - 1;

            Assert.NotNull(address);
            Assert.Equal(id, address.Id);
            Assert.Equal("test-address", address.Name);

            var another = World.AddressFactory.UniqueWith("another-address");

            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(id, address.GetHashCode());
        }

        [Fact]
        public void TestNameAndIdGiven()
        {
            const long id = 123;

            var address = World.AddressFactory.From(id, "test-address");

            Assert.NotNull(address);
            Assert.Equal(id, address.Id);
            Assert.Equal("test-address", address.Name);

            var another = World.AddressFactory.From(456, "test-address");

            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(address, World.AddressFactory.From(123, "test-address"));
            Assert.Equal(0, address.CompareTo(World.AddressFactory.From(123, "test-address")));
            Assert.Equal(id, address.GetHashCode());
        }
    }
}
