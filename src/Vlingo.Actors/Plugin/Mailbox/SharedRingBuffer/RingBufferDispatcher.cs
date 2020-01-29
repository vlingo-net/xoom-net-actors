// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferDispatcher : IRunnable, IDispatcher
    {
        private readonly Backoff backoff;
        private readonly AtomicBoolean closed;
        private readonly int throttlingCount;

        private CancellationTokenSource backoffTokenSource;
        private readonly CancellationTokenSource dispatcherTokenSource;
        private Task? started;
        private readonly object mutex = new object();
        
        internal RingBufferDispatcher(int mailboxSize, long fixedBackoff, int throttlingCount)
        {
            closed = new AtomicBoolean(false);
            backoff = fixedBackoff == 0L ? new Backoff() : new Backoff(fixedBackoff);
            RequiresExecutionNotification = fixedBackoff == 0L;
            Mailbox = new SharedRingBufferMailbox(this, mailboxSize);
            this.throttlingCount = throttlingCount;
            dispatcherTokenSource = new CancellationTokenSource();
            backoffTokenSource = CancellationTokenSource.CreateLinkedTokenSource(dispatcherTokenSource.Token);
        }

        internal IMailbox Mailbox { get; private set; }

        public bool RequiresExecutionNotification { get; private set; }

        public void Close()
        {
            if (!IsClosed)
            {
                closed.Set(true);
                Mailbox.Close();
                dispatcherTokenSource.Cancel();
                dispatcherTokenSource.Dispose();
                backoffTokenSource.Dispose();
            }
        }
        
        public bool IsClosed => closed.Get();

        public void Execute(IMailbox mailbox)
        {
            backoffTokenSource.Cancel();
            backoffTokenSource.Dispose();
            backoffTokenSource = CancellationTokenSource.CreateLinkedTokenSource(dispatcherTokenSource.Token);
        }

        public void Start()
        {
            lock (mutex)
            {
                if (started != null)
                {
                    return;
                }

                started = Task.Run(() => Run(), dispatcherTokenSource.Token);
            }
        }

        public async void Run()
        {
            while (!IsClosed)
            {
                if (!Deliver())
                {
                    await backoff.Now(backoffTokenSource.Token);
                }
            }
        }

        private bool Deliver()
        {
            for (int idx = 0; idx < throttlingCount; ++idx)
            {
                var message = Mailbox.Receive();
                if (message == null)
                {
                    return idx > 0; // we delivered at least one message
                }
                
                if (!IsClosed)
                {
                    message.Deliver();
                }
            }
            return true;
        }
    }
}
