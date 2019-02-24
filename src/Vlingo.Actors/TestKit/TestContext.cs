// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors.TestKit
{
    /// <summary>
    /// A context useful for testing.
    /// </summary>
    public class TestContext
    {
        /// <summary>
        /// A reference to any object that may be of use to the test.
        /// Use reference() to cast the inner object to a specific type.
        /// </summary>
        private readonly AtomicReference<object> reference = new AtomicReference<object>();

        /// <summary>
        /// Track number of expected happenings. Use resetHappeningsTo(n)
        /// to change expectations inside a single test.
        /// </summary>
        public TestUntil Until { get; } = TestUntil.Happenings(0);

        public virtual T Reference<T>() => (T)reference.Get();

        public virtual void ResetHappeningsTo(int times) => Until.ResetHappeningsTo(times);
    }
}
