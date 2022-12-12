// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.TestKit
{
    public class TestkitTest : ActorsTest
    {
        [Fact]
        public void TestTesterWorldPing()
        {
            var pingCounter = TestWorld.ActorFor<IPingCounter>(Definition.Has<PingCounterActor>(Definition.NoParameters));
            pingCounter?.Actor.Ping();
            pingCounter?.Actor.Ping();
            pingCounter?.Actor.Ping();

            Assert.Equal(3, TestWorld.AllMessagesFor(pingCounter!.Address).Count);
            Assert.Equal(3, pingCounter.ViewTestState().ValueOf<int>("count"));
        }

        [Fact]
        public void TestTesterPingPong()
        {
            var pongCounter = TestWorld.ActorFor<IPongCounter>(Definition.Has<PongCounterActor>(Definition.NoParameters));
            var pingCounter = TestWorld.ActorFor<IPingCounter>(
                Definition.Has<PingPongCounterActor>(
                    Definition.Parameters(pongCounter!.Actor)));

            pingCounter?.Actor.Ping();
            pingCounter?.Actor.Ping();
            pingCounter?.Actor.Ping();

            Assert.Equal(3, TestWorld.AllMessagesFor(pingCounter!.Address).Count);
            Assert.Equal(3, TestWorld.AllMessagesFor(pongCounter.Address).Count);
            Assert.Equal(3, pingCounter.ViewTestState().ValueOf<int>("count"));
            Assert.Equal(3, pongCounter.ViewTestState().ValueOf<int>("count"));
        }
        
        [Fact]
        public void TestThatTestRuntimeDiscovererDiscoversTest()
        {
            Assert.True(TestRuntimeDiscoverer.IsUnderTest());
        }

        private class PingCounterActor : Actor, IPingCounter
        {
            private int _count;
            public void Ping() => ++_count;
            public override TestState ViewTestState() => new TestState().PutValue("count", _count);
        }
        private class PongCounterActor : Actor, IPongCounter
        {
            private int _count;
            public void Pong() => ++_count;
            public override TestState ViewTestState() => new TestState().PutValue("count", _count);
        }
        private class PingPongCounterActor : Actor, IPingCounter
        {
            private int _count;
            private readonly IPongCounter _pongCounter;

            public PingPongCounterActor(IPongCounter pongCounter)
            {
                _pongCounter = pongCounter;
                _count = 0;
            }

            public void Ping()
            {
                ++_count;
                if(_pongCounter != null)
                {
                    _pongCounter.Pong();
                }
            }

            public override TestState ViewTestState() => new TestState().PutValue("count", _count);
        }
    }

    public interface IPingCounter
    {
        void Ping();
    }
    public interface IPongCounter
    {
        void Pong();
    }
}