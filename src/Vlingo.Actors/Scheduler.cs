using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Vlingo.Actors
{
    public class Scheduler
    {
        private readonly Timer timer;
        private readonly ConcurrentBag<SchedulerTask> tasks;

        public ICancellable Schedule(IScheduled scheduled, object data, long delayBefore, long interval)
            => CreateAndStore(
                scheduled,
                data,
                TimeSpan.FromMilliseconds(delayBefore),
                TimeSpan.FromMilliseconds(interval),
                true);

        public ICancellable ScheduleOnce(IScheduled scheduled, object data, long delayBefore, long interval)
            => CreateAndStore(
                scheduled,
                data,
                TimeSpan.FromMilliseconds(delayBefore),
                TimeSpan.FromMilliseconds(Timeout.Infinite),
                false);


        internal Scheduler()
        {
            tasks = new ConcurrentBag<SchedulerTask>();
        }

        protected void Close()
        {
            foreach(var task in tasks)
            {
                task.Cancel();
            }
        }

        private SchedulerTask CreateAndStore(
            IScheduled scheduled,
            object data,
            TimeSpan delayBefore,
            TimeSpan interval,
            bool repeats)
        {
            var task = new SchedulerTask(scheduled, data, delayBefore, interval, repeats);
            tasks.Add(task);
            return task;
        }

        private class SchedulerTask : ICancellable
        {
            private readonly IScheduled scheduled;
            private readonly bool repeats;
            private Timer timer;
            private bool hasRun;

            public SchedulerTask(IScheduled scheduled, object data, TimeSpan delayBefore, TimeSpan interval, bool repeats)
            {
                this.scheduled = scheduled;
                this.repeats = repeats;
                hasRun = false;
                timer = new Timer(new TimerCallback(Tick), data, delayBefore, interval);
            }

            private void Tick(object data)
            {
                hasRun = true;
                scheduled.IntervalSignal(scheduled, data);

                if (!repeats)
                {
                    Cancel();
                }
            }

            public bool Cancel()
            {
                if (timer != null)
                {
                    using (timer)
                    {
                        timer.Change(-1, -1);
                    }
                    timer = null;
                }

                return repeats || !hasRun;
            }
        }
    }
}
