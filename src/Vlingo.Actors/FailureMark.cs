// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors
{
    public sealed class FailureMark
    {
        private long _startOfPeriod;
        private int _timedIntensity;

        public FailureMark() => Reset();

        internal bool FailedWithExcessiveFailures(long period, int intensity)
        {
            if (intensity == SupervisionStrategyConstants.ForeverIntensity)
            {
                return false;
            }

            if (intensity == 1)
            {
                return true;
            }

            var currentTime = DateTimeHelper.CurrentTimeMillis();

            if (_startOfPeriod == 0)
            {
                _startOfPeriod = currentTime;
                _timedIntensity = 1;
            }
            else
            {
                ++_timedIntensity;
            }

            var periodExceeded = _startOfPeriod - currentTime >= period;

            if (_timedIntensity > intensity && !periodExceeded)
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
            _startOfPeriod = 0;
            _timedIntensity = 0;
        }
    }
}