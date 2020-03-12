// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors.Tests.Plugin.Completes
{
    public class MockCompletesEventually : ICompletesEventually
    {
        public readonly CompletesResults completesResults;

        public MockCompletesEventually(CompletesResults completesResults)
        {
            this.completesResults = completesResults;
        }

        public bool IsStopped => false;

        public IAddress Address => null;

        public void Conclude()
        {
        }

        public void Stop()
        {
        }

        public void With(object outcome)
        {
            completesResults.Outcome.Set(outcome);
            completesResults.WithCount.IncrementAndGet();
        }

        public class CompletesResults
        {
            public AtomicReference<object> Outcome { get; } = new AtomicReference<object>();
            public AtomicInteger WithCount { get; } = new AtomicInteger(0);
        }
    }
}
