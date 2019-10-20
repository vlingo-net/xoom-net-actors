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
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailbox : IMailbox
    {
        public const string Name = "testerMailbox";

        private readonly IList<string> lifecycleMessages = new List<string> { "Start", "AfterStop", "BeforeRestart", "AfterRestart" };
        private readonly TestWorld world;
        private readonly ConcurrentQueue<IMessage> queue;
        private readonly AtomicReference<Stack<List<Type>>> suspendedOverrides;

        public TestMailbox()
        {
            world = TestWorld.Instance!;
            queue = new ConcurrentQueue<IMessage>();
            suspendedOverrides = new AtomicReference<Stack<List<Type>>>(new Stack<List<Type>>());
        }

        public void Run()
            => throw new NotSupportedException("TestMailbox does not support this operation.");

        public void Close()
        {
            IsClosed = true;
        }

        public bool IsClosed { get; private set; }

        public bool IsDelivering => throw new NotSupportedException("TestMailbox does not support this operation.");

        public bool IsPreallocated => false;

        public int PendingMessages => throw new NotSupportedException("TestMailbox does not support this operation.");

        public bool IsSuspended => suspendedOverrides.Get()!.Count > 0;

        public void Resume(string name)
        {
            if (suspendedOverrides.Get()!.Count > 0)
            {
                suspendedOverrides.Get()!.Pop();
            }
            ResumeAll();
        }

        public void Send(IMessage message)
        {
            if (!message.Actor.IsStopped)
            {
                if (!IsLifecycleMessage(message))
                {
                    world.Track(message);
                }
            }

            if (IsSuspended)
            {
                queue.Enqueue(message);
                return;
            }
            else
            {
                ResumeAll();
            }

            message.Actor.ViewTestStateInitialization(null);
            message.Deliver();
        }

        public IMessage Receive() => throw new NotSupportedException("TestMailbox does not support this operation.");

        private bool IsLifecycleMessage(IMessage message)
        {
            var representation = message.Representation;
            var openParenIndex = representation.IndexOf('(');
            return lifecycleMessages.Contains(representation.Substring(0, openParenIndex));
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
            => throw new NotSupportedException("Not a preallocated mailbox.");

        public void SuspendExceptFor(string name, params Type[] overrides)
            => suspendedOverrides.Get()!.Push(overrides.ToList());

        private void ResumeAll()
        {
            while (!queue.IsEmpty)
            {
                if(queue.TryDequeue(out var queued))
                {
                    var actor = queued.Actor;
                    if(actor != null)
                    {
                        actor.ViewTestStateInitialization(null);
                        queued.Deliver();
                    }
                }
            }
        }
    }
}
