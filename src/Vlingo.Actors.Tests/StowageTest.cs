// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Actors.Tests
{
    public class StowageTest
    {
        [Fact]
        public void TestStowHasMessages()
        {
            IMessage _;

            var stowage = new Stowage();
            stowage.StowingMode();

            stowage.Stow<object>(LocalMessage());
            Assert.True(stowage.HasMessages);

            _ = stowage.Head;
            Assert.False(stowage.HasMessages);

            stowage.Stow<object>(LocalMessage());
            stowage.Stow<object>(LocalMessage());
            Assert.True(stowage.HasMessages);

            _ = stowage.Head;
            Assert.True(stowage.HasMessages);

            _ = stowage.Head;
            Assert.False(stowage.HasMessages);
        }

        [Fact]
        public void TestHead()
        {
            var stowage = new Stowage();
            stowage.StowingMode();

            stowage.Stow<object>(LocalMessage());
            stowage.Stow<object>(LocalMessage());
            stowage.Stow<object>(LocalMessage());

            Assert.NotNull(stowage.Head);
            Assert.NotNull(stowage.Head);
            Assert.NotNull(stowage.Head);
            Assert.Null(stowage.Head);
        }

        [Fact]
        public void TestReset()
        {
            var stowage = new Stowage();
            stowage.StowingMode();

            Assert.True(stowage.IsStowing);
            Assert.False(stowage.IsDispersing);

            stowage.Stow<object>(LocalMessage());
            stowage.Stow<object>(LocalMessage());
            stowage.Stow<object>(LocalMessage());

            Assert.True(stowage.HasMessages);

            stowage.Reset();

            Assert.False(stowage.HasMessages);
            Assert.False(stowage.IsStowing);
            Assert.False(stowage.IsDispersing);
        }

        [Fact]
        public void TestDispersing()
        {
            var stowage = new Stowage();
            stowage.StowingMode();

            stowage.Stow<object>(LocalMessage("1"));
            stowage.Stow<object>(LocalMessage("2"));
            stowage.Stow<object>(LocalMessage("3"));

            Assert.True(stowage.HasMessages);

            stowage.DispersingMode();

            Assert.Equal("1", stowage.SwapWith<object>(LocalMessage("4")).Representation);
            Assert.Equal("2", stowage.SwapWith<object>(LocalMessage("5")).Representation);
            Assert.Equal("3", stowage.SwapWith<object>(LocalMessage("6")).Representation);

            Assert.True(stowage.HasMessages);
            Assert.True(stowage.IsDispersing);
            Assert.NotNull(stowage.Head);
            Assert.True(stowage.IsDispersing);
            Assert.NotNull(stowage.Head);
            Assert.True(stowage.IsDispersing);
            Assert.NotNull(stowage.Head);
            Assert.True(stowage.IsDispersing);
            Assert.Null(stowage.Head);
            Assert.False(stowage.IsDispersing);
        }

        private static IMessage LocalMessage() => new LocalMessage<object>(null, null, null, null);
        private static IMessage LocalMessage(string encode) => new LocalMessage<object>(null, null, null, encode);
    }
}
