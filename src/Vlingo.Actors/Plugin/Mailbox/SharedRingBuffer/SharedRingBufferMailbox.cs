// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class SharedRingBufferMailbox : IMailbox
    {
        private readonly AtomicBoolean closed;

        private readonly IDispatcher dispatcher;
        private readonly int mailboxSize;
        private readonly IMessage[] messages;
        private readonly AtomicLong sendIndex;
        private readonly AtomicLong readyIndex;
        private readonly AtomicLong receiveIndex;

        protected internal SharedRingBufferMailbox(IDispatcher dispatcher, int mailboxSize)
        {
            this.dispatcher = dispatcher;
            this.mailboxSize = mailboxSize;
            closed = new AtomicBoolean(false);
            messages = new IMessage[mailboxSize];
            readyIndex = new AtomicLong(-1);
            receiveIndex = new AtomicLong(-1);
            sendIndex = new AtomicLong(-1);

            InitPreallocated();
        }

        public virtual void Close()
        {
            if (!closed.Get())
            {
                closed.Set(true);
                dispatcher.Close();
            }
        }

        public virtual bool IsClosed => closed.Get();

        public virtual bool IsDelivering 
            => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public virtual bool Delivering(bool flag)
            => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public virtual bool IsPreallocated => true;

        public int PendingMessages => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation");

        public virtual void Send(IMessage message) => throw new NotSupportedException("Use preallocated mailbox Send(Actor, ...).");

        public virtual void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
        {
            var messageIndex = sendIndex.IncrementAndGet();
            var ringSendIndex = (int)(messageIndex % mailboxSize);
            int retries = 0;
            while (ringSendIndex == (int)(receiveIndex.Get() % mailboxSize))
            {
                if (++retries >= mailboxSize)
                {
                    if (closed.Get())
                    {
                        return;
                    }

                    retries = 0;
                }
            }

            messages[ringSendIndex].Set(actor, consumer, completes, representation);
            while (readyIndex.CompareAndSet(messageIndex - 1, messageIndex))
            { }
        }

        public virtual IMessage Receive()
        {
            var messageIndex = receiveIndex.Get();
            if (messageIndex < readyIndex.Get())
            {
                var index = (int)(receiveIndex.IncrementAndGet() % mailboxSize);
                return messages[index];
            }

            return null;
        }

        public virtual void Run() => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        private void InitPreallocated()
        {
            for (int idx = 0; idx < mailboxSize; ++idx)
            {
                messages[idx] = new LocalMessage<object>(this);
            }
        }
    }
}
