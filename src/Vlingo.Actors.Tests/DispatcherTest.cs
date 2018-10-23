// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class DispatcherTest : ActorsTest
    {
        private const int Total100Thousand = 100_000;

        [Fact]
        public void Test100MillionTells()
        {
            var test = TestWorld.ActorFor<ITellSomething>(Definition.Has<TellSomethingActor>(Definition.NoParameters, "test"));
            for (var i = 0; i < Total100Thousand; ++i)
            {
                test.Actor.TellMeSomething("Hello!", i);
            }

            Assert.Equal(Total100Thousand, TestWorld.AllMessagesFor(test.Address).Count);
            Assert.Equal(Total100Thousand, test.ViewTestState().ValueOf<int>("times"));
        }

        [Fact]
        public void Test100MillionTellWhatITellYou()
        {
            var test = TestWorld.ActorFor<ITellAll>(Definition.Has<TellAllActor>(Definition.NoParameters, "test"));
            for (var i = 0; i < Total100Thousand; ++i)
            {
                test.Actor.TellWhatITellYou(i);
            }

            Assert.Equal(Total100Thousand, TestWorld.AllMessagesFor(test.Address).Count);
            Assert.Equal(Total100Thousand - 1, test.ViewTestState().ValueOf<int>("lastValue"));
        }


        private class TellAllActor : Actor, ITellAll
        {
            private int lastValue;
            public void TellWhatITellYou(int value) => lastValue = value;
            public override TestState ViewTestState() => new TestState().PutValue("lastValue", lastValue);
        }

        private class TellSomethingActor : Actor, ITellSomething
        {
            private int times = 0;
            public void TellMeSomething(string something, int value) => ++times;
            public override TestState ViewTestState() => new TestState().PutValue("times", times);
        }
    }

    public interface ITellAll
    {
        void TellWhatITellYou(int value);
    }

    public interface ITellSomething
    {
        void TellMeSomething(string something, int value);
    }
}
