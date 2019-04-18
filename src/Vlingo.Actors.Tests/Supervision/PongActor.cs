// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class PongActor : Actor, IPong
    {
        public static ThreadLocal<PongActor> Instance = new ThreadLocal<PongActor>();

        private readonly PongTestResults testResults;

        public PongActor(PongTestResults testResults)
        {
            this.testResults = testResults;
            Instance.Value = this;
        }

        public void Pong()
        {
            testResults.PongCount.IncrementAndGet();
            testResults.UntilPonged.Happened();
            throw new ApplicationException("Intended Pong failure.");
        }

        public override void Stop()
        {
            base.Stop();
            testResults.UntilStopped.Happened();
        }

        public class PongTestResults
        {
            public readonly AtomicInteger PongCount = new AtomicInteger(0);
            public readonly AtomicInteger StopCount = new AtomicInteger(0);

            public AccessSafely Access { get; private set; }

            public PongTestResults()
            {
                Access = AfterCompleting(0);
            }

            public AccessSafely AfterCompleting(int times)
            {
                Access = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("pongCount", (int increment) => PongCount.Set(PongCount.Get() + increment))
                    .ReadingWith("pongCount", () => PongCount.Get())
                    .WritingWith("stopCount", (int increment) => StopCount.Set(StopCount.Get() + increment))
                    .ReadingWith("stopCount", () => StopCount.Get());
                return Access;
            }
        }
    }
}
