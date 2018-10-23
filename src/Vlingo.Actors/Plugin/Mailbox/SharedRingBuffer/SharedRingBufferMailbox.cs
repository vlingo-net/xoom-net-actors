// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class SharedRingBufferMailbox : IMailbox
    {
        private readonly IDispatcher dispatcher;
        private readonly int mailboxSize;
        private readonly OverflowQueue overflowQueue;
        private readonly IMessage[] messages;
        private int sendIndex;
        private int receiveIndex;

        internal SharedRingBufferMailbox(IDispatcher dispatcher, int mailboxSize)
        {
            this.dispatcher = dispatcher;
            this.mailboxSize = mailboxSize;
            overflowQueue = new OverflowQueue(this);
            messages = new IMessage[mailboxSize];
            receiveIndex = 0;
            sendIndex = 0;
        }

        public void Close()
        {
            if (!IsClosed)
            {
                IsClosed = true;
                dispatcher.Close();
                overflowQueue.Close();
                Clear();
            }
        }

        public bool IsClosed { get; private set; }

        public bool IsDelivering => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public bool Delivering(bool flag) => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        public int OverflowCount => overflowQueue.Count;

        public void Send(IMessage message)
        {
            lock (messages)
            {
                if(messages[sendIndex] == null)
                {
                    messages[sendIndex] = message;
                    if(++sendIndex >= mailboxSize)
                    {
                        sendIndex = 0;
                    }

                    if (dispatcher.RequiresExecutionNotification)
                    {
                        dispatcher.Execute(this);
                    }
                }
                else
                {
                    overflowQueue.DelayedSend(message);
                    dispatcher.Execute(this);
                }
            }
        }

        public IMessage Receive()
        {
            var message = messages[receiveIndex];
            if (message != null)
            {
                messages[receiveIndex] = null;
                if (++receiveIndex >= mailboxSize)
                {
                    receiveIndex = 0;
                }
                if (overflowQueue.IsOverflowed)
                {
                    overflowQueue.Execute();
                }
            }
            return message;
        }

        public void Run() => throw new NotSupportedException("SharedRingBufferMailbox does not support this operation.");

        private bool CanSend()
        {
            var index = sendIndex;
            if (index >= mailboxSize)
            {
                index = 0;
            }

            return messages[index] == null;
        }

        private void Clear()
        {
            for (int idx = 0; idx < mailboxSize; ++idx)
            {
                messages[idx] = null;
            }
        }

        private class OverflowQueue : IRunnable
        {
            private Backoff backoff;
            private Queue<IMessage> messages;
            private bool open;

            internal OverflowQueue(SharedRingBufferMailbox parent)
            {
                backoff = new Backoff();
                messages = new Queue<IMessage>();
                open = false;
                this.parent = parent;
            }

            public void Run()
            {
                while (open)
                {
                    if (parent.CanSend())
                    {
                        
                        if (messages.TryDequeue(out IMessage delayed))
                        {
                            backoff.Reset();
                            parent.Send(delayed);
                        }
                        else
                        {
                            backoff.Now();
                        }
                    }
                    else
                    {
                        backoff.Now();
                    }
                }
            }

            private Thread _internalThread;
            private readonly object _threadMutex = new object();
            private readonly SharedRingBufferMailbox parent;

            private void Start()
            {
                lock (_threadMutex)
                {
                    if(_internalThread != null)
                    {
                        return;
                    }
                    _internalThread = new Thread(Run);
                    _internalThread.Start();
                }
            }

            internal int Count => messages.Count;

            public bool IsOverflowed => open && messages.Count > 0;

            internal void Close()
            {
                open = false;
                messages.Clear();
            }

            internal void DelayedSend(IMessage message)
            {
                messages.Enqueue(message);
                if (!open)
                {
                    open = true;
                    Start();
                }
                else
                {
                    Execute();
                }
            }

            internal void Execute()
            {
                if (_internalThread != null)
                {
                    _internalThread.Interrupt();
                }
            }
        }
    }
}
