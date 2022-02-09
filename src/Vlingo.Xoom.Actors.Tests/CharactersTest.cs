// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
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
            internal readonly AccessSafely Counters;

            public Results(int times)
            {
                var one = new AtomicInteger(0);
                var two = new AtomicInteger(0);
                var three = new AtomicInteger(0);

                Counters = AccessSafely.AfterCompleting(times);

                Counters.WritingWith<int>("one", x => one.AddAndGet(x));
                Counters.ReadingWith("one", one.Get);
                Counters.WritingWith<int>("two", x => two.AddAndGet(x));
                Counters.ReadingWith("two", two.Get);
                Counters.WritingWith<int>("three", x => three.AddAndGet(x));
                Counters.ReadingWith("three", three.Get);
            }

            public int GetCounterValue(string name) => Counters.ReadFrom<int>(name);
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
                results.Counters.WriteUsing("one", incrementBy);
                characters.Become(TWO);
            }

            public void Two()
            {
                results.Counters.WriteUsing("two", incrementBy);
                characters.Become(THREE);
            }

            public void Three()
            {
                results.Counters.WriteUsing("three", incrementBy);
                characters.Become(ONE);
            }
        }

        private class ThreeBehaviorsActor : Actor, IThreeBehaviors
        {
            private readonly Characters<IThreeBehaviors> _characters;

            public ThreeBehaviorsActor(Results results)
            {
                var one = new ThreeBehaviorsState(results, 1);
                var two = new ThreeBehaviorsState(results, 2);
                var three = new ThreeBehaviorsState(results, 3);
                _characters = new Characters<IThreeBehaviors>(new List<IThreeBehaviors>
                {
                    one, two, three
                });
                one.SetCharacters(_characters);
                two.SetCharacters(_characters);
                three.SetCharacters(_characters);
            }

            public void One() => _characters.Current.One();

            public void Two() => _characters.Current.Two();

            public void Three() => _characters.Current.Three();
        }
    }

    public interface IThreeBehaviors
    {
        void One();
        void Two();
        void Three();
    }
}
