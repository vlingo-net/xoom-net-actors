// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailbox : IMailbox
    {
        private readonly AtomicBoolean delivering;
        private readonly IDispatcher dispatcher;
        private readonly AtomicReference<SuspendedDeliveryOverrides> suspendedDeliveryOverrides;
        private readonly ConcurrentQueue<IMessage> queue;
        private readonly byte throttlingCount;

        internal ConcurrentQueueMailbox(IDispatcher dispatcher, int throttlingCount)
        {
            this.dispatcher = dispatcher;
            delivering = new AtomicBoolean(false);
            suspendedDeliveryOverrides = new AtomicReference<SuspendedDeliveryOverrides>(new SuspendedDeliveryOverrides());
            queue = new ConcurrentQueue<IMessage>();
            this.throttlingCount = (byte)throttlingCount;
        }

        public void Close()
        {
            // queue.Clear();
            dispatcher.Close();
        }

        public bool IsClosed => dispatcher.IsClosed;

        public void Resume(string name)
        {
            if (suspendedDeliveryOverrides.Get()!.Pop(name))
            {
                dispatcher.Execute(this);
            }
        }

        public void Send(IMessage message)
        {
            if (IsSuspended)
            {
                if (suspendedDeliveryOverrides.Get()!.MatchesTop(message.Protocol))
                {
                    dispatcher.Execute(new ResumingMailbox(message));
                    if (!queue.IsEmpty)
                    {
                        dispatcher.Execute(this);
                    }
                    return;
                }
                queue.Enqueue(message);
            }
            else
            {
                queue.Enqueue(message);
                if (!IsDelivering)
                {
                    dispatcher.Execute(this);
                }
            }
        }

        public void SuspendExceptFor(string name, params Type[] overrides)
            => suspendedDeliveryOverrides.Get()!.Push(new Overrides(name, overrides));

        public bool IsSuspended => !suspendedDeliveryOverrides.Get()!.IsEmpty;

        public IMessage? Receive()
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

        public void Run()
        {
            if(delivering.CompareAndSet(false, true))
            {
                var total = throttlingCount;
                for(var count = 0; count < total; ++count)
                {
                    if (IsSuspended)
                    {
                        break;
                    }

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
                delivering.Set(false);
                if (!queue.IsEmpty)
                {
                    dispatcher.Execute(this);
                }
            }
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
        {
            throw new NotSupportedException("Not a preallocated mailbox.");
        }


        private class SuspendedDeliveryOverrides
        {
            private readonly AtomicBoolean accessible;
            private readonly IList<Overrides> overrides;

            public SuspendedDeliveryOverrides()
            {
                accessible = new AtomicBoolean(false);
                overrides = new List<Overrides>();
            }

            public bool IsEmpty => overrides.Count == 0;

            public bool MatchesTop(Type messageType)
            {
                var overrides = Peek();

                if(overrides != null)
                {
                    return overrides.Types.Contains(messageType);
                }

                return false;
            }

            public Overrides? Peek()
            {
                var retries = 0;
                while (true)
                {
                    if(accessible.CompareAndSet(false, true))
                    {
                        Overrides? temp = null;
                        if (!IsEmpty)
                        {
                            temp = overrides[0];
                        }
                        accessible.Set(false);
                        return temp;
                    }

                    if(++retries > 100_000_000)
                    {
                        return null;
                    }
                }
            }

            public bool Pop(string name)
            {
                var popped = false;
                var retries = 0;
                while (true)
                {
                    if(accessible.CompareAndSet(false, true))
                    {
                        var elements = overrides.Count;
                        for(var index=0; index < elements; ++index)
                        {
                            if (name.Equals(overrides[index].Name))
                            {
                                if(index == 0)
                                {
                                    overrides.RemoveAt(index);
                                    popped = true;
                                    --elements;

                                    while(index < elements)
                                    {
                                        if (overrides[index].Obsolete)
                                        {
                                            overrides.RemoveAt(index);
                                            --elements;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    overrides[index].Obsolete = true;
                                }

                                accessible.Set(false);
                                break;
                            }
                        }

                        break;
                    }

                    if(++retries > 100_000_000)
                    {
                        return false;
                    }
                }

                return popped;
            }

            public void Push(Overrides overrides)
            {
                var retries = 0;

                while (true)
                {
                    if(accessible.CompareAndSet(false, true))
                    {
                        this.overrides.Add(overrides);
                        accessible.Set(false);
                        break;
                    }

                    if(++retries > 100_000_000)
                    {
                        return;
                    }
                }
            }

        }

        private class Overrides
        {
            public Overrides(string name, Type[] types)
            {
                Name = name;
                Types = types;
                Obsolete = false;
            }

            public string Name { get; }
            public Type[] Types { get; }
            public bool Obsolete { get; set; }
        }
    }
}
