// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.TestKit
{
    /// <summary>
    /// A context useful for testing, consisting of an atomic reference value
    /// and a safe access to state modification with expected number of outcomes.
    /// </summary>
    public class TestContext
    {
        public TestContext(int times)
        {
            Access = AccessSafely.AfterCompleting(times);
            _reference = new AtomicReference<object>();
            SetUpWriteRead();
        }

        public TestContext()
            : this(0)
        {
        }

        /// <summary>
        /// Answer the <typeparamref name="T"/> typed value of my access when it is available.
        /// Block unless the value is immediately available.
        /// </summary>
        /// <typeparam name="T">The type of my reference to answer.</typeparam>
        /// <returns></returns>
        public virtual T MustComplete<T>() => Access.ReadFrom<T>("reference");

        /// <summary>
        /// Answer myself after initializing my atomic reference to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="value">The value of type <typeparamref name="T"/>.</param>
        /// <returns></returns>
        public virtual TestContext InitialReferenceValueOf<T>(T value)
        {
            _reference.Set(value);
            return this;
        }

        /// <summary>
        /// A reference to any object that may be of use to the test.
        /// Use ReferenceValue&lt;T&gt;() to cast the inner object to a specific type.
        /// </summary>
        private readonly AtomicReference<object> _reference;

        /// <summary>
        /// Track number of expected happenings. Use resetHappeningsTo(n)
        /// to change expectations inside a single test.
        /// </summary>
        public TestUntil Until { get; } = TestUntil.Happenings(0);

        /// <summary>
        /// Answer my access;
        /// </summary>
        public AccessSafely Access { get; private set; }

        /// <summary>
        /// Answer my reference values as a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns></returns>
        public virtual T ReferenceValue<T>()
        {
            var result = _reference.Get();
            if (result == null)
            {
                return default!;
            }

            return (T) result;
        }

        /// <summary>
        /// Answer the <code>TestContext</code> after writing the value at <code>"reference"</code>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The <typeparamref name="T"/> typed value to write.</param>
        /// <returns></returns>
        public virtual TestContext ReferenceValueTo<T>(T value)
        {
            Access.WriteUsing("reference", value);
            return this;
        }

        /// <summary>
        /// Answer a myself with with a new expected completions/happenings <paramref name="times"/>.
        /// </summary>
        /// <param name="times">The number of expected completions.</param>
        /// <returns></returns>
        public virtual TestContext ResetAfterCompletingTo(int times)
        {
            Access = Access.ResetAfterCompletingTo(times);
            return this;
        }

        /// <summary>
        /// Set up writer and reader of state.
        /// </summary>
        private void SetUpWriteRead()
        {
            Access
                .WritingWith<object>("reference", value => _reference.Set(value))
                .ReadingWith("reference", () => _reference.Get());
        }
    }
}
