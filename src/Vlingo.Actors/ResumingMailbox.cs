// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class ResumingMailbox : IMailbox
    {
        private readonly IMessage message;

        public ResumingMailbox(IMessage message)
        {
            this.message = message;
        }

        public bool IsClosed => false;

        public bool IsDelivering => true;

        public bool IsSuspended => false;

        public int PendingMessages => 1;

        public bool IsPreallocated => false;

        public void Close()
        {
        }

        public IMessage Receive() => null;

        public void Resume(string name)
        {
        }

        public void Run() => message.Deliver();

        public void Send(IMessage message)
        {
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
        {
            throw new InvalidOperationException("Not a preallocated mailbox.");
        }

        public void SuspendExceptFor(string name, params Type[] overrides)
        {
        }
    }
}
