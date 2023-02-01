// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
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

            for (var idx = 0; idx < 10; ++idx)
            {
                protocols._1.Stow();
            }

            protocols._2.Override();

            Assert.Equal(1, results.OverrideAccess.ReadFrom<int>("overrideReceivedCount"));
            Assert.Equal(10, results.StowedAccess.ReadFrom<int>("stowReceivedCount"));
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

            Assert.Equal(1, results.OverrideAccess.ReadFrom<int>("overrideReceivedCount"));
            Assert.Equal(10, results.StowedAccess.ReadFrom<int>("stowReceivedCount"));
        }

        private class Results
        {
            public readonly AccessSafely OverrideAccess;
            public readonly AccessSafely StowedAccess;
            public readonly AtomicInteger OverrideReceivedCount;
            public readonly AtomicInteger StowReceivedCount;

            public Results(int overrideReceived, int stowReceived)
            {
                OverrideReceivedCount = new AtomicInteger(0);
                StowReceivedCount = new AtomicInteger(0);

                StowedAccess = AccessSafely
                    .AfterCompleting(stowReceived)
                    .WritingWith("stowReceivedCount", (int increment) => StowReceivedCount.Set(StowReceivedCount.Get() + increment))
                    .ReadingWith("stowReceivedCount", () => StowReceivedCount.Get());

                OverrideAccess = AccessSafely
                    .AfterCompleting(overrideReceived)
                    .WritingWith("overrideReceivedCount", (int increment) => OverrideReceivedCount.Set(OverrideReceivedCount.Get() + increment))
                    .ReadingWith("overrideReceivedCount", () => OverrideReceivedCount.Get());
            }
        }

        private class StowTestActor : Actor, IOverrideStowage, IStowThese
        {
            private readonly Results _results;

            public StowTestActor(Results results)
            {
                _results = results;
                StowMessages(typeof(IOverrideStowage));
            }

            public void Crash()
            {
                _results.OverrideAccess.WriteUsing("overrideReceivedCount", 1);
                throw new ApplicationException("Intended failure");
            }

            public void Override()
            {
                _results.OverrideAccess.WriteUsing("overrideReceivedCount", 1);
                DisperseStowedMessages();
            }

            public void Stow()
            {
                _results.StowedAccess.WriteUsing("stowReceivedCount", 1);
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
