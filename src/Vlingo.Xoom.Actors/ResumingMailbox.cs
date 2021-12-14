// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    public class ResumingMailbox : IMailbox
    {
        private readonly IMessage _message;

        public ResumingMailbox(IMessage message) => _message = message;

        public TaskScheduler TaskScheduler { get; } = null!;
        
        public bool IsClosed => false;

        public bool IsDelivering => true;
        public int ConcurrencyCapacity => 0;

        public bool IsSuspendedFor(string name) => throw new InvalidOperationException("Mailbox implementation does not support this operation.");

        public bool IsSuspended => false;

        public int PendingMessages => 1;

        public bool IsPreallocated => false;

        public void Close()
        {
        }

        public IMessage? Receive() => null;

        public void Resume(string name)
        {
        }

        public void Run() => _message.Deliver();

        public void Send(IMessage message)
        {
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation) => 
            throw new InvalidOperationException("Not a preallocated mailbox.");

        public void Send(Actor actor, Type protocol, LambdaExpression consumer, ICompletes? completes, string representation) => 
            throw new InvalidOperationException("Not a preallocated mailbox.");

        public void SuspendExceptFor(string name, params Type[] overrides)
        {
        }
    }
}
