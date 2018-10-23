// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Stowage
    {
        private Queue<IMessage> stowedMessages;
        private AtomicBoolean dispersing;
        private AtomicBoolean stowing;

        public Stowage()
        {
            dispersing = new AtomicBoolean(false);
            stowing = new AtomicBoolean(false);
            Reset();
        }

        protected internal int Count => stowedMessages.Count;

        protected internal void Dump(ILogger logger)
        {
            foreach (var message in stowedMessages)
            {
                logger.Log($"STOWED: {message}");
            }
        }

        protected internal bool HasMessages => stowedMessages.Count > 0;

        internal IMessage Head
        {
            get
            {
                if (!HasMessages)
                {
                    Reset();
                    return null;
                }

                return stowedMessages.Dequeue();
            }
        }

        protected internal void Reset()
        {
            stowedMessages = new Queue<IMessage>();
            stowing.Set(false);
            dispersing.Set(false);
        }

        protected internal bool IsStowing => stowing.Get();

        public void StowingMode()
        {
            stowing.Set(true);
            dispersing.Set(false);
        }

        protected internal bool IsDispersing => dispersing.Get();

        protected internal void DispersingMode()
        {
            stowing.Set(false);
            dispersing.Set(true);
        }

        protected internal void Stow<T>(IMessage message)
        {
            if (IsStowing)
            {
                stowedMessages.Enqueue(new StowedLocalMessage<T>((LocalMessage<T>)message));
            }
        }

        protected internal IMessage SwapWith<T>(IMessage newerMessage)
        {
            if (!HasMessages)
            {
                Reset();
                return newerMessage;
            }

            var olderMessage = stowedMessages.Dequeue();
            stowedMessages.Enqueue(new StowedLocalMessage<T>((LocalMessage<T>)newerMessage));
            return olderMessage;
        }
    }
}