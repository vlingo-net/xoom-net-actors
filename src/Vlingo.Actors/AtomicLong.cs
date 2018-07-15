// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicLong
    {
        private long value;

        public AtomicLong(long initialValue)
        {
            value = initialValue;
        }

        public long Get() => Interlocked.CompareExchange(ref value, 0, 0);

        public long IncrementAndGet() => Interlocked.Increment(ref value);

        public long GetAndIncrement() => Interlocked.Increment(ref value) - 1;
    }
}
