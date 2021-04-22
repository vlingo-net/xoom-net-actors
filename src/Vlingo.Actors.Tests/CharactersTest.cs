// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class CharactersTest : ActorsTest
    {

        [Fact]
        public void TestBecomeWithThreeCharacters()
        {
            var results = new Results(30);
            var threeBehaviors = World.ActorFor<IThreeBehaviors>(typeof(ThreeBehaviorsActor), results);

            for (var count = 0; count < 10; ++count)
            {
                threeBehaviors.One();
                threeBehaviors.Two();
                threeBehaviors.Three();
            }

            Assert.Equal(10, results.GetCounterValue("one"));
            Assert.Equal(20, results.GetCounterValue("two"));
            Assert.Equal(30, results.GetCounterValue("three"));
        }

        private class Results
        {
            internal readonly AccessSafely counters;

            public Results(int times)
            {
                var one = new AtomicInteger(0);
                var two = new AtomicInteger(0);
                var three = new AtomicInteger(0);

                counters = AccessSafely.AfterCompleting(times);

                counters.WritingWith<int>("one", x => one.AddAndGet(x));
                counters.ReadingWith("one", one.Get);
                counters.WritingWith<int>("two", x => two.AddAndGet(x));
                counters.ReadingWith("two", two.Get);
                counters.WritingWith<int>("three", x => three.AddAndGet(x));
                counters.ReadingWith("three", three.Get);
            }

            public int GetCounterValue(string name) => counters.ReadFrom<int>(name);
        }

        private class ThreeBehaviorsState : IThreeBehaviors
        {
            public const int ONE = 0, TWO = 1, THREE = 2;

            private readonly Results results;
            private readonly int incrementBy;
            private Characters<IThreeBehaviors> characters;

            public ThreeBehaviorsState(Results results, int incrementBy)
            {
                this.results = results;
                this.incrementBy = incrementBy;
            }

            public void SetCharacters(Characters<IThreeBehaviors> characters)
            {
                this.characters = characters;
            }

            public void One()
            {
                results.counters.WriteUsing("one", incrementBy);
                characters.Become(TWO);
            }

            public void Two()
            {
                results.counters.WriteUsing("two", incrementBy);
                characters.Become(THREE);
            }

            public void Three()
            {
                results.counters.WriteUsing("three", incrementBy);
                characters.Become(ONE);
            }
        }

        private class ThreeBehaviorsActor : Actor, IThreeBehaviors
        {
            private readonly Characters<IThreeBehaviors> characters;

            public ThreeBehaviorsActor(Results results)
            {
                var one = new ThreeBehaviorsState(results, 1);
                var two = new ThreeBehaviorsState(results, 2);
                var three = new ThreeBehaviorsState(results, 3);
                characters = new Characters<IThreeBehaviors>(new List<IThreeBehaviors>
                {
                    one, two, three
                });
                one.SetCharacters(characters);
                two.SetCharacters(characters);
                three.SetCharacters(characters);
            }

            public void One() => characters.Current.One();

            public void Two() => characters.Current.Two();

            public void Three() => characters.Current.Three();
        }
    }

    public interface IThreeBehaviors
    {
        void One();
        void Two();
        void Three();
    }
}
