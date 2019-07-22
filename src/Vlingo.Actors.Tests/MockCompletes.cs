// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests
{
    public class MockCompletes<T> : BasicCompletes<T> where T:class
    {
        private readonly AtomicReference<T> outcome = new AtomicReference<T>(default(T));
        private readonly AtomicInteger withCount = new AtomicInteger(0);
        private readonly AccessSafely safely;

        public MockCompletes(int times)
            : base((Scheduler)null)
        {
            safely = AccessSafely.AfterCompleting(times)
                .WritingWith<T>("outcome", val =>
                {
                    outcome.Set(val);
                    withCount.IncrementAndGet();
                })
                .ReadingWith("outcome", outcome.Get)
                .ReadingWith("count", withCount.Get);
        }

        public int WithCount => safely.ReadFrom<int>("count");

        public override T Outcome => safely.ReadFrom<T>("outcome");

        public override ICompletes<TOutcome> With<TOutcome>(TOutcome outcome)
        {
            safely.WriteUsing("outcome", (T)(object)outcome);
            return (ICompletes<TOutcome>)this;
        }
    }
}
