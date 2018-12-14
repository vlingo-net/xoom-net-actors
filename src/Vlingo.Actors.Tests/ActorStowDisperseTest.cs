// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorStowDisperseTest : ActorsTest
    {
        [Fact]
        public void TestThatStowedMessagesDisperseOnOverride()
        {
            var results = new Results(1, 10);
            var protocols = Protocols.Two<IStowThese, IOverrideStowage>(
                World.ActorFor(
                    Definition.Has<StowTestActor>(Definition.Parameters(results), "stow-override"),
                    new[] { typeof(IStowThese), typeof(IOverrideStowage) }));

            for(var idx=0; idx<10; ++idx)
            {
                protocols._1.Stow();
            }

            protocols._2.Override();

            results.overrideReceived.Completes();
            results.stowReceived.Completes();

            Assert.Equal(1, results.overrideReceivedCount);
            Assert.Equal(10, results.stowReceivedCount);
        }

        [Fact]
        public void TestThatStowedMessagesDisperseOnCrash()
        {
            var results = new Results(1, 10);
            var protocols = Protocols.Two<IStowThese, IOverrideStowage>(
                World.ActorFor(
                    Definition.Has<StowTestActor>(Definition.Parameters(results), "stow-override"),
                    new[] { typeof(IStowThese), typeof(IOverrideStowage) }));

            for (var idx = 0; idx < 10; ++idx)
            {
                protocols._1.Stow();
            }

            protocols._2.Crash();

            results.overrideReceived.Completes();
            results.stowReceived.Completes();

            Assert.Equal(1, results.overrideReceivedCount);
            Assert.Equal(10, results.stowReceivedCount);
        }

        private class Results
        {
            public readonly TestUntil overrideReceived;
            public int overrideReceivedCount;
            public readonly TestUntil stowReceived;
            public int stowReceivedCount;
            public Results(int overrideReceived, int stowReceived)
            {
                this.overrideReceived = TestUntil.Happenings(overrideReceived);
                this.stowReceived = TestUntil.Happenings(stowReceived);
            }
        }

        private class StowTestActor : Actor, IOverrideStowage, IStowThese
        {
            private readonly Results results;

            public StowTestActor(Results results)
            {
                this.results = results;
                StowMessages(typeof(IOverrideStowage));
            }

            public void Crash()
            {
                ++results.overrideReceivedCount;
                results.overrideReceived.Happened();
                throw new ApplicationException("Intended failure");
            }

            public void Override()
            {
                ++results.overrideReceivedCount;
                DisperseStowedMessages();
                results.overrideReceived.Happened();
            }

            public void Stow()
            {
                ++results.stowReceivedCount;
                results.stowReceived.Happened();
            }
        }
    }

    public interface IOverrideStowage
    {
        void Crash();
        void Override();
    }
    public interface IStowThese
    {
        void Stow();
    }
}
