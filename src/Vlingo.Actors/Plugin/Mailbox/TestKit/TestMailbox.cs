// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailbox : IMailbox
    {
        public const string Name = "testerMailbox";

        private readonly IList<string> lifecycleMessages = new List<string> { "Start", "AfterStop", "BeforeRestart", "AfterRestart" };
        private readonly TestWorld testWorld;

        public TestMailbox()
        {
            testWorld = TestWorld.testWorld;
        }

        public void Run()
            => throw new NotSupportedException("TestMailbox does not support this operation.");

        public void Close()
        {
            IsClosed = true;
        }

        public bool IsClosed { get; private set; }

        public bool IsDelivering => throw new NotSupportedException("TestMailbox does not support this operation.");

        public bool Delivering(bool flag) => throw new NotSupportedException("TestMailbox does not support this operation.");

        public void Send(IMessage message)
        {
            if (!message.Actor.IsStopped)
            {
                if (!IsLifecycleMessage(message))
                {
                    testWorld.Track(message);
                }
            }

            message.Deliver();
        }

        public IMessage Receive() => throw new NotSupportedException("TestMailbox does not support this operation.");

        private bool IsLifecycleMessage(IMessage message)
        {
            var representation = message.Representation;
            var openParenIndex = representation.IndexOf('(');
            return lifecycleMessages.Contains(representation.Substring(0, openParenIndex));
        }
    }
}
