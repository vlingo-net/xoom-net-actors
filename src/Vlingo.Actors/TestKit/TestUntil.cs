// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
        private readonly bool zero;

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
            if (zero)
            {
                try
                {
                    Thread.Sleep(10);
                }
                catch (Exception)
                {
                    // Ignore !
                }
            }
            else
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
        }

        public bool CompletesWithin(long timeout)
        {
            var countDown = timeout;
            while (true)
            {
                if (countDownEvent.IsSet)
                {
                    return true;
                }

                try
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds((countDown >= 0 && countDown < 100) ? countDown : 100));
                }
                catch (Exception)
                {
                }

                if (countDownEvent.IsSet)
                {
                    return true;
                }

                if (timeout >= 0)
                {
                    countDown -= 100;
                    if (countDown <= 0)
                    {
                        return false;
                    }
                }
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

        public override string ToString() => $"TestUntil[count={countDownEvent.CurrentCount} , zero={zero}]";
        
        public void Dispose() => countDownEvent.Dispose();

        private TestUntil(int count)
        {
            countDownEvent = new CountdownEvent(initialCount: count);

            zero = (count == 0);
        }
    }
}