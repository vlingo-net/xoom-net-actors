// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ProtocolsTest : ActorsTest
    {
        [Fact]
        public void TestTwoProtocols()
        {
            var protocols = TestWorld.ActorFor(
                new[] { typeof(P1), typeof(P2) },
                Definition.Has<TwoProtocolsActor>(Definition.NoParameters));

            var two = Protocols.Two<TestActor<P1>, TestActor<P2>>(protocols);

            two._1.Actor.Do1();
            Assert.Equal(1, TwoProtocolsActor.Instance.Value.Do1Count);

            two._2.Actor.Do2();
            two._2.Actor.Do2();
            Assert.Equal(2, TwoProtocolsActor.Instance.Value.Do2Count);
        }

        [Fact]
        public void TestThreeProtocols()
        {
            var protocols = TestWorld.ActorFor(
                new[] { typeof(P1), typeof(P2), typeof(P3) }, Definition.Has<ThreeProtocolsActor>(Definition.NoParameters));
            var three = Protocols.Three<TestActor<P1>, TestActor<P2>, TestActor<P3>>(protocols);

            three._1.Actor.Do1();
            Assert.Equal(1, ThreeProtocolsActor.Instance.Value.Do1Count);

            three._2.Actor.Do2();
            three._2.Actor.Do2();
            Assert.Equal(2, ThreeProtocolsActor.Instance.Value.Do2Count);

            three._3.Actor.Do3();
            three._3.Actor.Do3();
            three._3.Actor.Do3();
            Assert.Equal(3, ThreeProtocolsActor.Instance.Value.Do3Count);
        }

        [Fact]
        public void TestFourProtocols()
        {
            var protocols = TestWorld.ActorFor(
                new[] { typeof(P1), typeof(P2), typeof(P3), typeof(P4) }, Definition.Has<FourProtocolsActor>(Definition.NoParameters));
            var four = Protocols.Four<TestActor<P1>, TestActor<P2>, TestActor<P3>, TestActor<P4>>(protocols);

            four._1.Actor.Do1();
            Assert.Equal(1, FourProtocolsActor.Instance.Value.Do1Count);

            four._2.Actor.Do2();
            four._2.Actor.Do2();
            Assert.Equal(2, FourProtocolsActor.Instance.Value.Do2Count);

            four._3.Actor.Do3();
            four._3.Actor.Do3();
            four._3.Actor.Do3();
            Assert.Equal(3, FourProtocolsActor.Instance.Value.Do3Count);

            four._4.Actor.Do4();
            four._4.Actor.Do4();
            four._4.Actor.Do4();
            four._4.Actor.Do4();
            Assert.Equal(4, FourProtocolsActor.Instance.Value.Do4Count);
        }

        [Fact]
        public void TestFiveProtocols()
        {
            var protocols = TestWorld.ActorFor(
                new[] { typeof(P1), typeof(P2), typeof(P3), typeof(P4), typeof(P5) },
                Definition.Has<FiveProtocolsActor>(Definition.NoParameters));
            var four = Protocols.Five<TestActor<P1>, TestActor<P2>, TestActor<P3>, TestActor<P4>, TestActor<P5>>(protocols);

            four._1.Actor.Do1();
            Assert.Equal(1, FiveProtocolsActor.Instance.Value.Do1Count);

            four._2.Actor.Do2();
            four._2.Actor.Do2();
            Assert.Equal(2, FiveProtocolsActor.Instance.Value.Do2Count);

            four._3.Actor.Do3();
            four._3.Actor.Do3();
            four._3.Actor.Do3();
            Assert.Equal(3, FiveProtocolsActor.Instance.Value.Do3Count);

            four._4.Actor.Do4();
            four._4.Actor.Do4();
            four._4.Actor.Do4();
            four._4.Actor.Do4();
            Assert.Equal(4, FiveProtocolsActor.Instance.Value.Do4Count);

            four._5.Actor.Do5();
            four._5.Actor.Do5();
            four._5.Actor.Do5();
            four._5.Actor.Do5();
            four._5.Actor.Do5();
            Assert.Equal(5, FiveProtocolsActor.Instance.Value.Do5Count);
        }

        private class TwoProtocolsActor : Actor, P1, P2
        {
            public static ThreadLocal<TwoProtocolsActor> Instance = new ThreadLocal<TwoProtocolsActor>();
            public TwoProtocolsActor()
            {
                Instance.Value = this;
            }
            public int Do1Count { get; private set; }
            public int Do2Count { get; private set; }
            public void Do1() => ++Do1Count;
            public void Do2() => ++Do2Count;
        }

        private class ThreeProtocolsActor : Actor, P1, P2, P3
        {
            public static ThreadLocal<ThreeProtocolsActor> Instance = new ThreadLocal<ThreeProtocolsActor>();
            public ThreeProtocolsActor()
            {
                Instance.Value = this;
            }
            public int Do1Count { get; private set; }
            public int Do2Count { get; private set; }
            public int Do3Count { get; private set; }
            public void Do1() => ++Do1Count;
            public void Do2() => ++Do2Count;
            public void Do3() => ++Do3Count;
        }

        private class FourProtocolsActor : Actor, P1, P2, P3, P4
        {
            public static ThreadLocal<FourProtocolsActor> Instance = new ThreadLocal<FourProtocolsActor>();
            public FourProtocolsActor()
            {
                Instance.Value = this;
            }
            public int Do1Count { get; private set; }
            public int Do2Count { get; private set; }
            public int Do3Count { get; private set; }
            public int Do4Count { get; private set; }
            public void Do1() => ++Do1Count;
            public void Do2() => ++Do2Count;
            public void Do3() => ++Do3Count;
            public void Do4() => ++Do4Count;
        }
        private class FiveProtocolsActor : Actor, P1, P2, P3, P4, P5
        {
            public static ThreadLocal<FiveProtocolsActor> Instance = new ThreadLocal<FiveProtocolsActor>();
            public FiveProtocolsActor()
            {
                Instance.Value = this;
            }
            public int Do1Count { get; private set; }
            public int Do2Count { get; private set; }
            public int Do3Count { get; private set; }
            public int Do4Count { get; private set; }
            public int Do5Count { get; private set; }
            public void Do1() => ++Do1Count;
            public void Do2() => ++Do2Count;
            public void Do3() => ++Do3Count;
            public void Do4() => ++Do4Count;
            public void Do5() => ++Do5Count;
        }
    }

    public interface P1 { void Do1(); }
    public interface P2 { void Do2(); }
    public interface P3 { void Do3(); }
    public interface P4 { void Do4(); }
    public interface P5 { void Do5(); }
}
