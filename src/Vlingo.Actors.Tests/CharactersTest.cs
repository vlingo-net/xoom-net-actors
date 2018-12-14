// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class CharactersTest : IDisposable
    {
        private readonly World world;

        public CharactersTest()
        {
            world = World.StartWithDefault("become-test");
        }

        public void Dispose() => world.Terminate();

        [Fact]
        public void TestBecomeWithThreeCharacters()
        {
            var results = new Results(30);
            var threeBehaviors = world.ActorFor<IThreeBehaviors>(
                Definition.Has<ThreeBehaviorsActor>(Definition.Parameters(results)));

            for (var count = 0; count < 10; ++count)
            {
                threeBehaviors.One();
                threeBehaviors.Two();
                threeBehaviors.Three();
            }

            results.until.Completes();

            Assert.Equal(10, results.one);
            Assert.Equal(20, results.two);
            Assert.Equal(30, results.three);
        }

        private class Results
        {
            public int one;
            public int two;
            public int three;
            public TestUntil until;
            public Results(int times)
            {
                until = TestUntil.Happenings(times);
            }
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
                results.one += incrementBy;
                characters.Become(TWO);
                results.until.Happened();
            }

            public void Two()
            {
                results.two += incrementBy;
                characters.Become(THREE);
                results.until.Happened();
            }

            public void Three()
            {
                results.three += incrementBy;
                characters.Become(ONE);
                results.until.Happened();
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
