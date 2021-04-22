// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
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
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailbox : IMailbox
    {
        public const string Name = "testerMailbox";

        private readonly IList<string> _lifecycleMessages = new List<string> { "Start", "AfterStop", "BeforeRestart", "AfterRestart" };
        private readonly TestWorld _world;
        private readonly ConcurrentQueue<IMessage> _queue;
        private readonly AtomicReference<Stack<List<Type>>> _suspendedOverrides;

        public TestMailbox()
        {
            _world = TestWorld.Instance!;
            _queue = new ConcurrentQueue<IMessage>();
            _suspendedOverrides = new AtomicReference<Stack<List<Type>>>(new Stack<List<Type>>());
        }

        public void Run()
            => throw new NotSupportedException("TestMailbox does not support this operation.");

        public TaskScheduler TaskScheduler { get; } = null!;
        
        public void Close()
        {
            IsClosed = true;
        }

        public bool IsClosed { get; private set; }

        public bool IsDelivering => throw new NotSupportedException("TestMailbox does not support this operation.");

        public bool IsPreallocated => false;

        public int PendingMessages => throw new NotSupportedException("TestMailbox does not support this operation.");

        public bool IsSuspendedFor(string name) => IsSuspended;

        public bool IsSuspended => _suspendedOverrides.Get()!.Count > 0;

        public void Resume(string name)
        {
            if (_suspendedOverrides.Get()!.Count > 0)
            {
                _suspendedOverrides.Get()!.Pop();
            }
            ResumeAll();
        }

        public void Send(IMessage message)
        {
            if (!message.Actor.IsStopped)
            {
                if (!IsLifecycleMessage(message))
                {
                    _world.Track(message);
                }
            }

            if (IsSuspended)
            {
                _queue.Enqueue(message);
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
            return _lifecycleMessages.Contains(representation.Substring(0, openParenIndex));
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
            => throw new NotSupportedException("Not a preallocated mailbox.");

        public void SuspendExceptFor(string name, params Type[] overrides)
            => _suspendedOverrides.Get()!.Push(overrides.ToList());

        private void ResumeAll()
        {
            while (!_queue.IsEmpty)
            {
                if(_queue.TryDequeue(out var queued))
                {
                    var actor = queued.Actor;
                    if (actor != null)
                    {
                        actor.ViewTestStateInitialization(null);
                        queued.Deliver();
                    }
                }
            }
        }
    }
}
