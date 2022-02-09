// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Tests
{
    public class MockCompletes<T> : BasicCompletes<T> where T:class
    {
        private readonly AtomicReference<T> _outcome = new AtomicReference<T>(default(T));
        private readonly AtomicInteger _withCount = new AtomicInteger(0);
        private readonly AccessSafely _safely;

        public MockCompletes(int times)
            : base((Scheduler)null)
        {
            _safely = AccessSafely.AfterCompleting(times)
                .WritingWith<T>("outcome", val =>
                {
                    _outcome.Set(val);
                    _withCount.IncrementAndGet();
                })
                .ReadingWith("outcome", _outcome.Get)
                .ReadingWith("count", _withCount.Get);
        }

        public int WithCount => _safely.ReadFrom<int>("count");

        public override T Outcome => _safely.ReadFrom<T>("outcome");

        public override ICompletes<TOutcome> With<TOutcome>(TOutcome outcome)
        {
            _safely.WriteUsing("outcome", (T)(object)outcome);
            return (ICompletes<TOutcome>)this;
        }
    }
}
