// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueDispatcher : IRunnable, IDispatcher
    {
        private readonly Backoff _backoff;
        private readonly int _throttlingCount;
        private readonly AtomicBoolean _closed;

        private CancellationTokenSource _backoffTokenSource;
        private readonly CancellationTokenSource _dispatcherTokenSource;
        private Task? _started;
        private readonly object _mutex = new object();

        internal ManyToOneConcurrentArrayQueueDispatcher(
            int mailboxSize,
            long fixedBackoff,
            bool notifyOnSend,
            int throttlingCount,
            int totalSendRetries)
        {
            _backoff = fixedBackoff == 0L ? new Backoff() : new Backoff(fixedBackoff);
            RequiresExecutionNotification = fixedBackoff == 0L;
            Mailbox = new ManyToOneConcurrentArrayQueueMailbox(this, mailboxSize, totalSendRetries, notifyOnSend);
            _throttlingCount = throttlingCount;
            _closed = new AtomicBoolean(false);
            _dispatcherTokenSource = new CancellationTokenSource();
            _backoffTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_dispatcherTokenSource.Token);
        }

        public void Close()
        {
            _closed.Set(true);
            _dispatcherTokenSource.Cancel();
            _dispatcherTokenSource.Dispose();
            _backoffTokenSource.Dispose();
        }

        public bool IsClosed => _closed.Get();

        public int ConcurrencyCapacity => 1;

        public void Execute(IMailbox mailbox)
        {
            _backoffTokenSource.Cancel();
            _backoffTokenSource.Dispose();
            _backoffTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_dispatcherTokenSource.Token);
        }

        public bool RequiresExecutionNotification { get; }

        public async void Run()
        {
            while (!IsClosed)
            {
                if (!Deliver())
                {
                    await _backoff.Now(_backoffTokenSource.Token);
                }
            }
        }

        public void Start()
        {
            lock (_mutex)
            {
                if (_started != null)
                {
                    return;
                }

                _started = Task.Run(() => Run(), _dispatcherTokenSource.Token);
            }
        }

        internal IMailbox Mailbox { get; }

        private bool Deliver()
        {
            for (int idx = 0; idx < _throttlingCount; ++idx)
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
