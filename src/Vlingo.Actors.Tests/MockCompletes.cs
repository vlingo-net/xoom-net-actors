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
    public class MockCompletes<T> : BasicCompletes<T>
    {
        private T outcome;

        public MockCompletes()
            : base((Scheduler)null)
        {
            UntilWith = TestUntil.Happenings(0);
            WithCount = 0;
        }

        public TestUntil UntilWith { get; set; }

        public int WithCount { get; private set; }

        public override T Outcome => outcome;

        public override ICompletes<TOutcome> With<TOutcome>(TOutcome outcome)
        {
            this.outcome = (T)(object)outcome;
            ++WithCount;
            UntilWith.Happened();
            return (ICompletes<TOutcome>)this;
        }
    }
}
