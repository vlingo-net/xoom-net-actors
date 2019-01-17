// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Task started;

        public bool IsClosed => closed.Get();

        internal IMailbox Mailbox { get; private set; }

        public bool RequiresExecutionNotification { get; private set; }

        public void Close()
        {
            closed.Set(true);
            Mailbox.Close();
        }

        public void Execute(IMailbox mailbox)
        {
            cancellationTokenSource.Cancel();
        }

        public void Start()
        {
            if (started != null)
            {
                return;
            }
            
            started =  Task.Run(() => Run(), cancellationTokenSource.Token);
        }

        public void Run()
        {
            while (!closed.Get())
            {
                if (!Deliver())
                {
                    backoff.Now();
                }
            }
        }

        internal RingBufferDispatcher(int mailboxSize, long fixedBackoff, int throttlingCount)
        {
            closed = new AtomicBoolean(false);
            backoff = fixedBackoff == 0L ? new Backoff() : new Backoff(fixedBackoff);
            RequiresExecutionNotification = fixedBackoff == 0L;
            Mailbox = new SharedRingBufferMailbox(this, mailboxSize);
            this.throttlingCount = throttlingCount;
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
                else
                {
                    message.Deliver();
                }
            }
            return true;
        }
    }
}
