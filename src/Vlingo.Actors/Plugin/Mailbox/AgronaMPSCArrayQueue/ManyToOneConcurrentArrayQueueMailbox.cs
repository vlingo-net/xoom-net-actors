// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueMailbox : IMailbox
    {
        private readonly IDispatcher dispatcher;
        private readonly BlockingCollection<IMessage> queue;
        private readonly int totalSendRetries;

        internal ManyToOneConcurrentArrayQueueMailbox(
            IDispatcher dispatcher,
            int mailboxSize,
            int totalSendRetries)
        {
            this.dispatcher = dispatcher;
            queue = new BlockingCollection<IMessage>(new ConcurrentQueue<IMessage>(), mailboxSize);
            this.totalSendRetries = totalSendRetries;
        }

        public void Close()
        {
            dispatcher.Close();
            queue.CompleteAdding();
            queue.Dispose();
        }

        public bool IsClosed => dispatcher.IsClosed;

        public bool IsDelivering
            => throw new NotSupportedException("ManyToOneConcurrentArrayQueueMailbox does not support this operation.");

        public bool Delivering(bool flag) 
            => throw new NotSupportedException("ManyToOneConcurrentArrayQueueMailbox does not support this operation.");

        public void Run()
            => throw new NotSupportedException("ManyToOneConcurrentArrayQueueMailbox does not support this operation.");

        public void Send(IMessage message)
        {
            for (int tries = 0; tries < totalSendRetries; ++tries)
            {
                if (queue.TryAdd(message))
                {
                    return;
                }
            }
            throw new InvalidOperationException("Count not enqueue message due to busy mailbox.");
        }

        public IMessage Receive() => queue.Take();
    }
}
