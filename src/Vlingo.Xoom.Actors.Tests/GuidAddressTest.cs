// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Common.Identity;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class GuidAddressTest : IDisposable
    {
        private static int _maxPings = 5000;

        private readonly World _world;

        [Fact]
        public void TestNameGiven()
        {
            var address = _world.AddressFactory.UniqueWith("test-address");

            Assert.NotNull(address);
            Assert.Equal("test-address", address.Name);

            var another = _world.AddressFactory.UniqueWith("another-address");

            Assert.NotEqual(another, address);
            Assert.Equal("another-address", another.Name);
        }
        
        [Fact]
        public void TestIdType()
        {
            var address = _world.AddressFactory.UniqueWith("test-address");

            Assert.NotNull(address);
            Assert.Equal(0, address.CompareTo(address));
            Assert.False(address.IsDistributable);

            var addressId = address.IdTyped(s => new Guid(s)); // asserts cast
            Assert.Equal(address.IdString, new Guid(address.IdString).ToString()); // asserts UUID compatibility

            var another = _world.AddressFactory.UniqueWith("another-address");

            Assert.NotEqual(another, address);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.False(another.IsDistributable);

            var anotherId = another.IdTyped(s => new Guid(s)); // asserts cast
            Assert.Equal(another.IdString, new Guid(another.IdString).ToString()); // asserts UUID compatibility

            Assert.NotEqual(addressId, anotherId);
        }
        
        [Fact]
        public void TestNameAndIdGiven()
        {
            var id = Guid.NewGuid().ToString();
            var address = _world.AddressFactory.From(id, "test-address");

            Assert.NotNull(address);
            Assert.Equal(id, address.IdString);
            Assert.Equal("test-address", address.Name);
            Assert.Equal(address, _world.AddressFactory.From(id, "test-address"));

            var anotherId = Guid.NewGuid().ToString();
            var another = _world.AddressFactory.From(anotherId, "test-address-1");

            Assert.NotEqual(address.Name, another.Name);
            Assert.NotEqual(0, address.CompareTo(another));
            Assert.Equal(0, another.CompareTo(_world.AddressFactory.From(anotherId, "test-address-1")));

            Assert.NotEqual(address.IdString, another.IdString);
            Assert.NotEqual(address, another);
        }

        [Fact]
        public void TestThatActorsAreOperational()
        {
            var testResults = new GuidTestResults();

            var access = testResults.AfterCompleting(_maxPings * 2);

            var ping = _world.ActorFor<IPing>(() => new GuidPingActor(testResults, _maxPings));

            ping.Ping();

            var pingCount = access.ReadFrom<int>("pingCount");
            var pongCount = access.ReadFrom<int>("pongCount");

            Assert.Equal(pingCount, pongCount);
        }

        public GuidAddressTest()
        {
            var addressFactory =
                new GuidAddressFactory(IdentityGeneratorType.Random);

            _world =
                World.Start(
                    "test-address",
                    Configuration.Define().With(addressFactory));
        }
        
        public void Dispose()
        {
            _world.Terminate();
        }
    }
    
    public interface IPing : IStoppable
    {
        void Ping();
    }
    
    internal class GuidPingActor : Actor, IPing
    {
        private readonly IPing _ping;
        private int _pings;
        private readonly IPong _pong;
        private readonly GuidTestResults _testResults;
        private readonly int _maxPings;

        public GuidPingActor(GuidTestResults testResults, int maxPings)
        {
            _testResults = testResults;
            _maxPings = maxPings;
            _pong = ChildActorFor<IPong>(() => new GuidPongActor(testResults));
            _pings = 0;
            _ping = SelfAs<IPing>();
        }

        public void Ping()
        {
            _testResults.Access.WriteUsing("pingCount", 1);

            var stop = (++_pings >= _maxPings);

            if (stop) _ping.Stop();

            _pong.Pong(_ping);

            if (stop) _pong.Stop();
        }
    }
    
    internal class GuidPongActor : Actor, IPong
    {
        private readonly GuidTestResults _testResults;

        public GuidPongActor(GuidTestResults testResults) => _testResults = testResults;

        public void Pong(IPing ping)
        {
            _testResults.Access.WriteUsing("pongCount", 1);
            ping.Ping();
        }
    }
    
    public interface IPong : IStoppable
    {
        void Pong(IPing ping);
    }

    internal class GuidTestResults
    {
        public AccessSafely Access { get; private set; } = AccessSafely.AfterCompleting(0);

        public AtomicInteger PingCount => new AtomicInteger(0);
        public AtomicInteger PongCount => new AtomicInteger(0);

        public AccessSafely AfterCompleting(int times)
        {
            Access =
                AccessSafely.AfterCompleting(times)
                    .WritingWith<int>("pingCount", increment => PingCount.IncrementAndGet())
                    .ReadingWith("pingCount", () => PingCount.Get())

                    .WritingWith<int>("pongCount", increment => PongCount.IncrementAndGet())
                    .ReadingWith("pongCount", () => PongCount.Get());

            return Access;
        }
    }
}