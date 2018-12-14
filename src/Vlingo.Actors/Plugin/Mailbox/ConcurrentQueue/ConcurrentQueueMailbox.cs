// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailbox : IMailbox, IRunnable
    {
        private readonly IDispatcher dispatcher;
        private readonly AtomicBoolean delivering;
        private readonly ConcurrentQueue<IMessage> queue;
        private readonly byte throttlingCount;

        internal ConcurrentQueueMailbox(IDispatcher dispatcher, int throttlingCount)
        {
            this.dispatcher = dispatcher;
            delivering = new AtomicBoolean(false);
            queue = new ConcurrentQueue<IMessage>();
            this.throttlingCount = (byte)throttlingCount;
        }

        public void Close()
        {
            // queue.Clear();
            dispatcher.Close();
        }

        public bool IsClosed => dispatcher.IsClosed;

        public void Send(IMessage message)
        {
            queue.Enqueue(message);
            if (!IsDelivering)
            {
                dispatcher.Execute(this);
            }
        }

        public IMessage Receive()
        {
            if(queue.TryDequeue(out IMessage result))
            {
                return result;
            }

            return null;
        }

        public bool IsDelivering => delivering.Get();

        public bool IsPreallocated => false;

        public int PendingMessages => queue.Count;

        public bool Delivering(bool flag) => delivering.CompareAndSet(!flag, flag);

        public void Run()
        {
            var total = (int)throttlingCount;
            for(var count = 0; count < total; ++count)
            {
                var message = Receive();
                if(message != null)
                {
                    message.Deliver();
                }
                else
                {
                    break;
                }
            }
            Delivering(false);

            if (!queue.IsEmpty)
            {
                dispatcher.Execute(this);
            }
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
        {
            throw new NotSupportedException("Not a preallocated mailbox.");
        }
    }
}
