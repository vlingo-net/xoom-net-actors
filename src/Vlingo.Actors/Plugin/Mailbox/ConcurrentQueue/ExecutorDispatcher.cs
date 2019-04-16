// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ExecutorDispatcher : IDispatcher
    {
        private readonly AtomicBoolean closed;
        private readonly ThreadPoolExecutor executor;

        internal ExecutorDispatcher(int availableThreads, float numberOfDispatchersFactor)
        {
            var maxAllowedConcurrentThreads = (int)(availableThreads * numberOfDispatchersFactor);
            closed = new AtomicBoolean(false);
            executor = new ThreadPoolExecutor(maxAllowedConcurrentThreads, HandleRejection);
        }

        public void Close()
        {
            closed.Set(true);
            executor.Shutdown();
        }

        public bool IsClosed => closed.Get();

        public bool RequiresExecutionNotification => false;

        public void Execute(IMailbox mailbox)
        {
            if (!IsClosed)
            {
                executor.Execute(mailbox);
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
}
