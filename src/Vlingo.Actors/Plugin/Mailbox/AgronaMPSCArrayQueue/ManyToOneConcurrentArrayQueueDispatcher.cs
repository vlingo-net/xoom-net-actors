// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueDispatcher : IRunnable, IDispatcher
    {
        private readonly Backoff backoff;
        private readonly int throttlingCount;
        private readonly AtomicBoolean closed;

        internal ManyToOneConcurrentArrayQueueDispatcher(
            int mailboxSize,
            long fixedBackoff,
            int throttlingCount,
            int totalSendRetries)
        {
            backoff = fixedBackoff == 0L ? new Backoff() : new Backoff(fixedBackoff);
            RequiresExecutionNotification = fixedBackoff == 0L;
            Mailbox = new ManyToOneConcurrentArrayQueueMailbox(this, mailboxSize, totalSendRetries);
            this.throttlingCount = throttlingCount;
            closed = new AtomicBoolean(false);
        }

        public void Close() => closed.Set(true);

        public bool IsClosed => closed.Get();

        public void Execute(IMailbox mailbox)
        {
            if (_thread != null)
            {
                _thread.Interrupt();
            }
        }

        public bool RequiresExecutionNotification { get; }

        public void Run()
        {
            while (!IsClosed)
            {
                if (!Deliver())
                {
                    backoff.Now();
                }
            }
        }

        private Thread _thread;
        private readonly object _threadMutex = new object();
        public void Start()
        {
            lock (_threadMutex)
            {
                if(_thread != null)
                {
                    return;
                }

                _thread = new Thread(Run);
                _thread.Start();
            }
        }

        internal IMailbox Mailbox { get; }

        private bool Deliver()
        {
            for (int idx = 0; idx < throttlingCount; ++idx)
            {
                var message = Mailbox.Receive();
                if (message == null)
                {
                    return idx > 0; // we delivered at least one message
                }
                else
                {
                    message.Deliver();
                }
            }
            return true;
        }
    }
}
