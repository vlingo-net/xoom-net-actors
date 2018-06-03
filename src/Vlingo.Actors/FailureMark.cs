using System;

namespace Vlingo.Actors
{
    public class FailureMark
    {
        private static readonly DateTime Since1970 = new DateTime(1970, 1, 1);
        private long startOfPeriod;
        private int timedIntensity;

        public FailureMark()
        {
            Reset();
        }

        protected bool FailedWithExcessiveFailures(long period, int intensity)
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
            else if (periodExceeded)
            {
                Reset();
                return FailedWithExcessiveFailures(period, intensity);
            }

            return false;
        }

        protected void Reset()
        {
            startOfPeriod = 0;
            timedIntensity = 0;
        }

        private static long CurrentTimeMillies => 
            (long)(DateTime.Now - Since1970).TotalMilliseconds;
    }
}