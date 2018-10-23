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
        private readonly CountdownEvent countDown;
        private readonly bool zero;

        public static TestUntil Happenings(int times) => new TestUntil(count: times);

        public void CompleteNow()
        {
            while (!countDown.IsSet)
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
                    countDown.Wait();
                }
                catch (Exception)
                {
                    // Ignore !
                }
            }
        }

        public TestUntil Happened()
        {
            if (!countDown.IsSet)
            {
                countDown.Signal();
            }

            return this;
        }

        public int Remaining => countDown.CurrentCount;

        public override string ToString() => $"TestUntil[count={countDown.CurrentCount} , zero={zero}]";
        
        public void Dispose() => countDown.Dispose();

        private TestUntil(int count)
        {
            countDown = new CountdownEvent(initialCount: count);

            zero = (count == 0);
        }
    }
}