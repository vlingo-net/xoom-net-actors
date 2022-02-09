// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Xoom.Actors.TestKit
{
    public class TestUntil : IDisposable
    {
        private readonly CountdownEvent _countDownEvent;

        public static TestUntil Happenings(int times) => new TestUntil(count: times);

        public void CompleteNow()
        {
            while (!_countDownEvent.IsSet)
            {
                Happened();
            }
        }

        public void Completes()
        {
            try
            {
                _countDownEvent.Wait();
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
                _countDownEvent.Wait(TimeSpan.FromMilliseconds(timeout));
                return _countDownEvent.CurrentCount == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TestUntil Happened()
        {
            if (!_countDownEvent.IsSet)
            {
                _countDownEvent.Signal();
            }

            return this;
        }

        public int Remaining => _countDownEvent.CurrentCount;

        public void ResetHappeningsTo(int times) => _countDownEvent.Reset(times);

        public override string ToString() => $"TestUntil[count={_countDownEvent.CurrentCount}]";
        
        public void Dispose() => _countDownEvent.Dispose();

        private TestUntil(int count)
        {
            _countDownEvent = new CountdownEvent(initialCount: count);
        }
    }
}