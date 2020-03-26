// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class SharedRingBufferMailbox : IMailbox
    {
        private readonly AtomicBoolean _closed;

        private readonly IDispatcher _dispatcher;
        private readonly int _mailboxSize;
        private readonly IMessage[] _messages;
        private readonly AtomicLong _sendIndex;
        private readonly AtomicLong _readyIndex;
        private readonly AtomicLong _receiveIndex;

        protected internal SharedRingBufferMailbox(IDispatcher dispatcher, int mailboxSize)
        {
            _dispatcher = dispatcher;
            _mailboxSize = mailboxSize;
            _closed = new AtomicBoolean(false);
            _messages = new IMessage[mailboxSize];
            _readyIndex = new AtomicLong(-1);
            _receiveIndex = new AtomicLong(-1);
            _sendIndex = new AtomicLong(-1);

            InitPreallocated();
        }

        public virtual void Close()
        {
            if (!_closed.Get())
            {
                _closed.Set(true);
                _dispatcher.Close();
            }
        }

        public TaskScheduler TaskScheduler { get; } = null!;

        public virtual bool IsClosed => _closed.Get();

        public virtual bool IsDelivering 
            => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public virtual bool IsPreallocated => true;

        public int PendingMessages => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation");

        public bool IsSuspended => false;

        public virtual void Send(IMessage message) => throw new NotSupportedException("Use preallocated mailbox Send(Actor, ...).");

        public virtual void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
        {
            var messageIndex = _sendIndex.IncrementAndGet();
            var ringSendIndex = (int)(messageIndex % _mailboxSize);
            int retries = 0;
            while (ringSendIndex == (int)(_receiveIndex.Get() % _mailboxSize))
            {
                if (++retries >= _mailboxSize)
                {
                    if (_closed.Get())
                    {
                        return;
                    }

                    retries = 0;
                }
            }

            _messages[ringSendIndex].Set(actor, consumer, completes, representation);
            while (_readyIndex.CompareAndSet(messageIndex - 1, messageIndex))
            { }
        }

        public virtual IMessage? Receive()
        {
            var messageIndex = _receiveIndex.Get();
            if (messageIndex < _readyIndex.Get())
            {
                var index = (int)(_receiveIndex.IncrementAndGet() % _mailboxSize);
                return _messages[index];
            }

            return null;
        }

        public virtual void Run() => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        private void InitPreallocated()
        {
            for (int idx = 0; idx < _mailboxSize; ++idx)
            {
                _messages[idx] = new LocalMessage<object>();
            }
        }

        public void Resume(string name) => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public void SuspendExceptFor(string name, params Type[] overrides)
            => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");
    }
}
