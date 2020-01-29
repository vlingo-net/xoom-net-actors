// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Actors.TestKit
{
    public class TestUntil : IDisposable
    {
        private readonly CountdownEvent countDownEvent;

        public static TestUntil Happenings(int times) => new TestUntil(count: times);

        public void CompleteNow()
        {
            while (!countDownEvent.IsSet)
            {
                Happened();
            }
        }

        public void Completes()
        {
            try
            {
                countDownEvent.Wait();
            }
            catch (Exception)
            {
                // Ignore !
            }
        }

        public bool CompletesWithin(long timeout)
        {
            try
            {
                countDownEvent.Wait(TimeSpan.FromMilliseconds(timeout));
                return countDownEvent.CurrentCount == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TestUntil Happened()
        {
            if (!countDownEvent.IsSet)
            {
                countDownEvent.Signal();
            }

            return this;
        }

        public int Remaining => countDownEvent.CurrentCount;

        public void ResetHappeningsTo(int times) => countDownEvent.Reset(times);

        public override string ToString() => $"TestUntil[count={countDownEvent.CurrentCount}]";
        
        public void Dispose() => countDownEvent.Dispose();

        private TestUntil(int count)
        {
            countDownEvent = new CountdownEvent(initialCount: count);
        }
    }
}