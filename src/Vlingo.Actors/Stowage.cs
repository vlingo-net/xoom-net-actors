// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Common;

namespace Vlingo.Actors
{
    internal class Stowage
    {
        private Queue<IMessage> stowedMessages;
        private bool dispersing;
        private bool stowing;

        public Stowage()
        {
            dispersing = false;
            stowing = false;
            Reset();
        }

        public override string ToString()
            => $"Stowage[stowing={stowing}, dispersing={dispersing}, messages={stowedMessages}]";

        internal int Count => stowedMessages.Count;

        internal void Dump(ILogger logger)
        {
            foreach (var message in stowedMessages)
            {
                logger.Log($"STOWED: {message}");
            }
        }

        internal bool HasMessages => stowedMessages.Count > 0;

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

        internal void Reset()
        {
            stowedMessages = new Queue<IMessage>();
            stowing = false;
            dispersing = false;
        }

        internal bool IsStowing => stowing;

        internal void StowingMode()
        {
            stowing = true;
            dispersing = false;
        }

        internal bool IsDispersing => dispersing;

        internal void DispersingMode()
        {
            stowing = false;
            dispersing = true;
        }

        internal void Restow(Stowage other)
        {
            var message = Head;
            while (message != null)
            {
                other.Stow(message);
                message = Head;
            }
        }

        internal void Stow(IMessage message)
        {
            if (IsStowing)
            {
                IMessage toStow = null;
                if (message.IsStowed)
                {
                    toStow = message;
                }
                else
                {
                    var closedMsgType = message.GetType().GetGenericArguments().First();
                    var stowedLocalMsgType = typeof(StowedLocalMessage<>).MakeGenericType(closedMsgType);
                    toStow = (IMessage)Activator.CreateInstance(stowedLocalMsgType, message);
                }

                stowedMessages.Enqueue(toStow);
            }
        }

        internal IMessage SwapWith<T>(IMessage newerMessage)
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