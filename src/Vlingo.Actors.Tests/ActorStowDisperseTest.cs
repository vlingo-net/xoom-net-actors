// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
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
                    new[] { typeof(IStowThese), typeof(IOverrideStowage) },
                    Definition.Has<StowTestActor>(Definition.Parameters(results), "stow-override")));

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
                    new[] { typeof(IStowThese), typeof(IOverrideStowage) },
                    Definition.Has<StowTestActor>(Definition.Parameters(results), "stow-override")));

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
            public readonly AccessSafely overrideAccess;
            public readonly AccessSafely stowedAccess;
            public readonly AtomicInteger overrideReceivedCount;
            public readonly AtomicInteger stowReceivedCount;

            public Results(int overrideReceived, int stowReceived)
            {
                overrideReceivedCount = new AtomicInteger(0);
                stowReceivedCount = new AtomicInteger(0);

                stowedAccess = AccessSafely
                    .AfterCompleting(stowReceived)
                    .WritingWith("stowReceivedCount", (int increment) => stowReceivedCount.Set(stowReceivedCount.Get() + increment))
                    .ReadingWith("stowReceivedCount", () => stowReceivedCount.Get());

                overrideAccess = AccessSafely
                    .AfterCompleting(overrideReceived)
                    .WritingWith("overrideReceivedCount", (int increment) => overrideReceivedCount.Set(overrideReceivedCount.Get() + increment))
                    .ReadingWith("overrideReceivedCount", () => overrideReceivedCount.Get());
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
                results.overrideAccess.WriteUsing("overrideReceivedCount", 1);
                throw new ApplicationException("Intended failure");
            }

            public void Override()
            {
                results.overrideAccess.WriteUsing("overrideReceivedCount", 1);
                DisperseStowedMessages();
            }

            public void Stow()
            {
                results.stowedAccess.WriteUsing("stowReceivedCount", 1);
            }

            protected internal override void BeforeResume(Exception reason)
            {
                DisperseStowedMessages();
                base.BeforeResume(reason);
            }

            protected internal override void AfterRestart(Exception reason)
            {
                DisperseStowedMessages();
                base.AfterRestart(reason);
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
