// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;

public class ConcurrentQueueMailbox : IMailbox
{
    private readonly AtomicBoolean _delivering;
    private readonly IDispatcher _dispatcher;
    private readonly AtomicReference<ExecutorDispatcherAsync> _dispatcherAsync;
    private readonly AtomicReference<SuspendedDeliveryOverrides> _suspendedDeliveryOverrides;
    private readonly ConcurrentQueue<IMessage> _queue;
    private readonly byte _throttlingCount;

    protected internal ConcurrentQueueMailbox(IDispatcher dispatcher, int throttlingCount)
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

    public int ConcurrencyCapacity => _dispatcher.ConcurrencyCapacity;

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

    public bool IsSuspendedFor(string name) => !_suspendedDeliveryOverrides.Get()!.Find(name).Any();

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

    public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation) => 
        throw new NotSupportedException("Not a preallocated mailbox.");

    public void Send(Actor actor, Type protocol, LambdaExpression consumer, ICompletes? completes, string representation) => 
        throw new NotSupportedException("Not a preallocated mailbox.");
}