// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;

public class ExecutorDispatcher : IDispatcher
{
    private readonly AtomicBoolean _closed;
    private readonly ThreadPoolExecutor _executor;
    private readonly int _maxAllowedConcurrentThreads;

    internal ExecutorDispatcher(int availableThreads, int numberOfDispatchers, float numberOfDispatchersFactor)
    {
        _maxAllowedConcurrentThreads = numberOfDispatchers > 0 ? 
            numberOfDispatchers : (int)(availableThreads * numberOfDispatchersFactor);
        _closed = new AtomicBoolean(false);
        _executor = new ThreadPoolExecutor(_maxAllowedConcurrentThreads, HandleRejection);
    }

    public void Close()
    {
        _closed.Set(true);
        _executor.Shutdown();
    }

    public bool IsClosed => _closed.Get();

    public bool RequiresExecutionNotification => false;
        
    public int ConcurrencyCapacity => _maxAllowedConcurrentThreads;

    public void Execute(IMailbox mailbox)
    {
        if (!IsClosed)
        {
            _executor.Execute(mailbox);
        }
    }

    private void HandleRejection(IRunnable task)
    {
        if (!IsClosed)
        {
            throw new InvalidOperationException("Message cannot be sent due to current system resource limitations.");
        }
    }
}