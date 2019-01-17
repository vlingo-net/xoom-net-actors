// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public sealed class FailureMark
    {
        private static readonly DateTime Since1970 = new DateTime(1970, 1, 1);
        private long startOfPeriod;
        private int timedIntensity;

        public FailureMark()
        {
            Reset();
        }

        internal bool FailedWithExcessiveFailures(long period, int intensity)
        {
            if (intensity == SupervisionStrategyConstants.ForeverIntensity)
            {
                return false;
            }
            else if (intensity == 1)
            {
                return true;
            }

            var currentTime = CurrentTimeMillies;

            if (startOfPeriod == 0)
            {
                startOfPeriod = currentTime;
                timedIntensity = 1;
            }
            else
            {
                ++timedIntensity;
            }

            bool periodExceeded = startOfPeriod - currentTime >= period;

            if (timedIntensity > intensity && !periodExceeded)
            {
                return true;
            }

            if (periodExceeded)
            {
                Reset();
                return FailedWithExcessiveFailures(period, intensity);
            }

            return false;
        }

        private void Reset()
        {
            startOfPeriod = 0;
            timedIntensity = 0;
        }

        private static long CurrentTimeMillies => 
            (long)(DateTime.Now - Since1970).TotalMilliseconds;
    }
}