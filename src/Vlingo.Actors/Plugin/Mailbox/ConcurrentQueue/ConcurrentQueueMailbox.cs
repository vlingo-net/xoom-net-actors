// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailbox : IMailbox
    {
        private readonly AtomicBoolean _delivering;
        private readonly IDispatcher _dispatcher;
        private readonly AtomicReference<ExecutorDispatcherAsync> _dispatcherAsync;
        private readonly AtomicReference<SuspendedDeliveryOverrides> _suspendedDeliveryOverrides;
        private readonly ConcurrentQueue<IMessage> _queue;
        private readonly byte _throttlingCount;

        internal ConcurrentQueueMailbox(IDispatcher dispatcher, int throttlingCount)
        {
            _dispatcher = dispatcher;
            _dispatcherAsync = new AtomicReference<ExecutorDispatcherAsync>(new ExecutorDispatcherAsync(this));
            _delivering = new AtomicBoolean(false);
            _suspendedDeliveryOverrides = new AtomicReference<SuspendedDeliveryOverrides>(new SuspendedDeliveryOverrides());
            _queue = new ConcurrentQueue<IMessage>();
            _throttlingCount = (byte)throttlingCount;
        }

        public TaskScheduler TaskScheduler => _dispatcherAsync.Get()!;

        public void Close()
        {
            while (_queue.TryDequeue(out _))
            {
                // do nothing
            }
        }

        public bool IsClosed => _dispatcher.IsClosed;

        public void Resume(string name)
        {
            if (_suspendedDeliveryOverrides.Get()!.Pop(name))
            {
                _dispatcher.Execute(this);
            }
        }

        public void Send(IMessage message)
        {
            if (IsSuspended)
            {
                if (_suspendedDeliveryOverrides.Get()!.MatchesTop(message.Protocol))
                {
                    _dispatcher.Execute(new ResumingMailbox(message));
                    if (!_queue.IsEmpty)
                    {
                        _dispatcher.Execute(this);
                    }
                    return;
                }
                _queue.Enqueue(message);
            }
            else
            {
                _queue.Enqueue(message);
                if (!IsDelivering)
                {
                    _dispatcher.Execute(this);
                }
            }
        }

        public void SuspendExceptFor(string name, params Type[] overrides)
            => _suspendedDeliveryOverrides.Get()!.Push(new Overrides(name, overrides));

        public bool IsSuspended => !_suspendedDeliveryOverrides.Get()!.IsEmpty;

        public IMessage? Receive()
        {
            if(_queue.TryDequeue(out var result))
            {
                return result;
            }

            return null;
        }

        public bool IsDelivering => _delivering.Get();

        public bool IsPreallocated => false;

        public int PendingMessages => _queue.Count;

        public void Run()
        {
            if(_delivering.CompareAndSet(false, true))
            {
                var total = _throttlingCount;
                for(var count = 0; count < total; ++count)
                {
                    if (IsSuspended)
                    {
                        break;
                    }

                    var message = Receive();

                    if (message != null)
                    {
                        message.Deliver();
                    }
                    else
                    {
                        break;
                    }
                }
                _delivering.Set(false);
                if (!_queue.IsEmpty)
                {
                    _dispatcher.Execute(this);
                }
            }
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
        {
            throw new NotSupportedException("Not a preallocated mailbox.");
        }

        private class SuspendedDeliveryOverrides
        {
            private readonly AtomicBoolean _accessible;
            private readonly IList<Overrides> _overrides;

            public SuspendedDeliveryOverrides()
            {
                _accessible = new AtomicBoolean(false);
                _overrides = new List<Overrides>();
            }

            public bool IsEmpty => _overrides.Count == 0;

            public bool MatchesTop(Type messageType)
            {
                var overrides = Peek();

                if (overrides != null)
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
                    if(_accessible.CompareAndSet(false, true))
                    {
                        Overrides? temp = null;
                        if (!IsEmpty)
                        {
                            temp = _overrides[0];
                        }
                        _accessible.Set(false);
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
                    if(_accessible.CompareAndSet(false, true))
                    {
                        var elements = _overrides.Count;
                        for(var index=0; index < elements; ++index)
                        {
                            if (name.Equals(_overrides[index].Name))
                            {
                                if(index == 0)
                                {
                                    _overrides.RemoveAt(index);
                                    popped = true;
                                    --elements;

                                    while(index < elements)
                                    {
                                        if (_overrides[index].Obsolete)
                                        {
                                            _overrides.RemoveAt(index);
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
                                    _overrides[index].Obsolete = true;
                                }

                                _accessible.Set(false);
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
                    if(_accessible.CompareAndSet(false, true))
                    {
                        this._overrides.Add(overrides);
                        _accessible.Set(false);
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
