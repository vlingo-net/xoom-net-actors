// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailbox : IMailbox
    {
        public const string Name = "testerMailbox";

        private readonly IList<string> lifecycleMessages = new List<string> { "Start", "AfterStop", "BeforeRestart", "AfterRestart" };
        private readonly TestWorld world;

        public TestMailbox()
        {
            world = TestWorld.Instance;
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

        public bool Delivering(bool flag) => throw new NotSupportedException("TestMailbox does not support this operation.");

        public void Send(IMessage message)
        {
            try
            {
                if (!message.Actor.IsStopped)
                {
                    if (!IsLifecycleMessage(message))
                    {
                        world.Track(message);
                    }
                }
                message.Actor.ViewTestStateInitialization(null);
                message.Deliver();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public IMessage Receive() => throw new NotSupportedException("TestMailbox does not support this operation.");

        private bool IsLifecycleMessage(IMessage message)
        {
            var representation = message.Representation;
            var openParenIndex = representation.IndexOf('(');
            return lifecycleMessages.Contains(representation.Substring(0, openParenIndex));
        }

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
        {
            throw new NotSupportedException("Not a preallocated mailbox.");
        }
    }
}
