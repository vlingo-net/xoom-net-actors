// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    internal class ThreadPoolExecutor
    {
        private readonly int maxConcurrentThreads;
        private readonly Action<IRunnable> rejectionHandler;
        private readonly ConcurrentQueue<IRunnable> queue;
        private readonly AtomicBoolean isShuttingDown;

        public ThreadPoolExecutor(int maxConcurrentThreads, Action<IRunnable> rejectionHandler)
        {
            this.maxConcurrentThreads = maxConcurrentThreads;
            this.rejectionHandler = rejectionHandler;
            queue = new ConcurrentQueue<IRunnable>();
            isShuttingDown = new AtomicBoolean(false);
        }

        public void Execute(IRunnable task)
        {
            if (isShuttingDown.Get())
            {
                rejectionHandler.Invoke(task);
                return;
            }

            queue.Enqueue(task);
            TryStartExecution();
        }

        public void Shutdown()
        {
            isShuttingDown.Set(true);
        }

        private void TryStartExecution()
        {
            if (!queue.IsEmpty && TryIncreaseRunningThreadCount())
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ => ThreadStartMethod()));
            }
        }

        private void ThreadStartMethod()
        {
            if(queue.TryDequeue(out IRunnable task))
            {
                try
                {
                    task.Run();
                }
                finally
                {
                    DecreaseRunningThreadCount();
                    TryStartExecution();
                }
            }
        }

        private int _currentThreadCount = 0;
        private bool TryIncreaseRunningThreadCount()
        {
            int currentCountLocal = Interlocked.CompareExchange(ref _currentThreadCount, 0, 0);
            while (currentCountLocal < maxConcurrentThreads)
            {
                var valueAtTheTimeOfIncrement = Interlocked.CompareExchange(ref _currentThreadCount, currentCountLocal + 1, currentCountLocal);
                if (valueAtTheTimeOfIncrement == currentCountLocal)
                {
                    return true;
                }

                currentCountLocal = Interlocked.CompareExchange(ref _currentThreadCount, 0, 0);
            }

            return false;
        }

        private void DecreaseRunningThreadCount() => Interlocked.Decrement(ref _currentThreadCount);
    }
}
