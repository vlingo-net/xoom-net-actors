// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class PongActor : Actor, IPong
    {
        public static ThreadLocal<PongActor> Instance = new ThreadLocal<PongActor>();

        private readonly PongTestResults _testResults;

        public PongActor(PongTestResults testResults)
        {
            _testResults = testResults;
            Instance.Value = this;
        }

        public void Pong()
        {
            _testResults.Access.WriteUsing("pongCount", 1);
            throw new ApplicationException("Intended Pong failure.");
        }

        public override void Stop()
        {
            base.Stop();
            _testResults.Access.WriteUsing("stopCount", 1);
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
