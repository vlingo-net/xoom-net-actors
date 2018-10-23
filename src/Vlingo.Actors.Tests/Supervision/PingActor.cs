// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors.Tests.Supervision
{
    public class PingActor : Actor, IPing
    {
        public static ThreadLocal<PingActor> Instance = new ThreadLocal<PingActor>();

        private readonly PingTestResults testResults;

        public PingActor(PingTestResults testResults)
        {
            this.testResults = testResults;
            Instance.Value = this;
        }

        public void Ping()
        {
            testResults.PingCount.IncrementAndGet();
            testResults.UntilPinged.Happened();
            throw new ApplicationException("Intended Ping failure.");
        }

        public override void Stop()
        {
            base.Stop();
            testResults.UntilStopped.Happened();
        }

        public class PingTestResults
        {
            public AtomicInteger PingCount = new AtomicInteger(0);
            public TestUntil UntilPinged = TestUntil.Happenings(0);
            public TestUntil UntilStopped = TestUntil.Happenings(0);
        }
    }
}
