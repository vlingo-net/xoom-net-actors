// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
        private readonly int _maxConcurrentThreads;
        private readonly Action<IRunnable> _rejectionHandler;
        private readonly ConcurrentQueue<IRunnable> _queue;
        private readonly AtomicBoolean _isShuttingDown;

        public ThreadPoolExecutor(int maxConcurrentThreads, Action<IRunnable> rejectionHandler)
        {
            _maxConcurrentThreads = maxConcurrentThreads;
            _rejectionHandler = rejectionHandler;
            _queue = new ConcurrentQueue<IRunnable>();
            _isShuttingDown = new AtomicBoolean(false);
        }

        public void Execute(IRunnable task)
        {
            if (_isShuttingDown.Get())
            {
                _rejectionHandler.Invoke(task);
                return;
            }

            _queue.Enqueue(task);
            TryStartExecution();
        }

        public void Shutdown()
        {
            _isShuttingDown.Set(true);
        }

        private void TryStartExecution()
        {
            if (!_queue.IsEmpty && TryIncreaseRunningThreadCount())
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ => ThreadStartMethod()));
            }
        }

        private void ThreadStartMethod()
        {
            if (_queue.TryDequeue(out IRunnable? task))
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

        private int _currentThreadCount;
        
        private bool TryIncreaseRunningThreadCount()
        {
            int currentCountLocal = Interlocked.CompareExchange(ref _currentThreadCount, 0, 0);
            while (currentCountLocal < _maxConcurrentThreads)
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