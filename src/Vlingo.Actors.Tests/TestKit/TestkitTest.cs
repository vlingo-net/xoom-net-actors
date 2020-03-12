// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.TestKit
{
    public class TestkitTest : ActorsTest
    {
        [Fact]
        public void TestTesterWorldPing()
        {
            var pingCounter = TestWorld.ActorFor<IPingCounter>(Definition.Has<PingCounterActor>(Definition.NoParameters));
            pingCounter.Actor.Ping();
            pingCounter.Actor.Ping();
            pingCounter.Actor.Ping();

            Assert.Equal(3, TestWorld.AllMessagesFor(pingCounter.Address).Count);
            Assert.Equal(3, pingCounter.ViewTestState().ValueOf<int>("count"));
        }

        [Fact]
        public void TestTesterPingPong()
        {
            var pongCounter = TestWorld.ActorFor<IPongCounter>(Definition.Has<PongCounterActor>(Definition.NoParameters));
            var pingCounter = TestWorld.ActorFor<IPingCounter>(
                Definition.Has<PingPongCounterActor>(
                    Definition.Parameters(pongCounter.Actor)));

            pingCounter.Actor.Ping();
            pingCounter.Actor.Ping();
            pingCounter.Actor.Ping();

            Assert.Equal(3, TestWorld.AllMessagesFor(pingCounter.Address).Count);
            Assert.Equal(3, TestWorld.AllMessagesFor(pongCounter.Address).Count);
            Assert.Equal(3, pingCounter.ViewTestState().ValueOf<int>("count"));
            Assert.Equal(3, pongCounter.ViewTestState().ValueOf<int>("count"));
        }

        private class PingCounterActor : Actor, IPingCounter
        {
            private int count = 0;
            public void Ping() => ++count;
            public override TestState ViewTestState() => new TestState().PutValue("count", count);
        }
        private class PongCounterActor : Actor, IPongCounter
        {
            private int count = 0;
            public void Pong() => ++count;
            public override TestState ViewTestState() => new TestState().PutValue("count", count);
        }
        private class PingPongCounterActor : Actor, IPingCounter
        {
            private int count;
            private readonly IPongCounter pongCounter;

            public PingPongCounterActor(IPongCounter pongCounter)
            {
                this.pongCounter = pongCounter;
                count = 0;
            }

            public void Ping()
            {
                ++count;
                if(pongCounter != null)
                {
                    pongCounter.Pong();
                }
            }

            public override TestState ViewTestState() => new TestState().PutValue("count", count);
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