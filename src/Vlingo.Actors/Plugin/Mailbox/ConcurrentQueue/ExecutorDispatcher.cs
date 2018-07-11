using System;
using System.Threading;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ExecutorDispatcher : IDispatcher
    {
        private readonly AtomicBoolean closed;
        private readonly int maxAllowedConcurrentThreads;

        internal ExecutorDispatcher(int availableThreads, float numberOfDispatchersFactor)
        {
            maxAllowedConcurrentThreads = (int)(availableThreads * numberOfDispatchersFactor);
            closed = new AtomicBoolean(false);
        }

        public void Close() => closed.Set(true);

        public bool IsClosed => closed.Get();

        public bool RequiresExecutionNotification => false;

        public void Execute(IMailbox mailbox)
        {
            if (IsClosed)
            {
                return;
            }

            if (mailbox.Delivering(true))
            {
                TryExecute(mailbox);
            }
        }

        private void TryExecute(IRunnable task)
        {
            if (CanQueueAnyMoreInThreadPool())
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ => ThreadStartMethod(task)));
            }
            else
            {
                HandleRejection(task);
            }
        }

        private void ThreadStartMethod(IRunnable task)
        {
            try
            {
                task.Run();
            }
            finally
            {
                DecrementRunningThreadCount();
            }
        }

        private void HandleRejection(IRunnable task)
        {
            if (!IsClosed)
            {
                throw new InvalidOperationException("Message cannot be sent due to current system resource limitations.");
            }
        }

        private int _currentThreadCount = 0;

        private bool CanQueueAnyMoreInThreadPool()
        {
            return TryIncrementRunningThreadCount();
        }

        private bool TryIncrementRunningThreadCount()
        {
            int currentCountLocal = Volatile.Read(ref _currentThreadCount);
            while (currentCountLocal < maxAllowedConcurrentThreads)
            {
                var valueAtTheTimeOfIncrement = Interlocked.CompareExchange(ref _currentThreadCount, currentCountLocal + 1, currentCountLocal);
                if(valueAtTheTimeOfIncrement == currentCountLocal)
                {
                    return true;
                }

                currentCountLocal = Volatile.Read(ref _currentThreadCount);
            }

            return false;
        }

        private void DecrementRunningThreadCount() => Interlocked.Decrement(ref _currentThreadCount);
    }
}
