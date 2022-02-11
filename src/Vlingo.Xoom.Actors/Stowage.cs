// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Actors;

internal class Stowage
{
    private Queue<IMessage>? _stowedMessages;
    private volatile bool _dispersing;
    private volatile bool _stowing;

    public Stowage()
    {
        _dispersing = false;
        _stowing = false;
        Reset();
    }

    public override string ToString()
        => $"Stowage[stowing={_stowing}, dispersing={_dispersing}, messages={_stowedMessages}]";

    internal int Count => _stowedMessages!.Count;

    internal void Dump(ILogger logger)
    {
        foreach (var message in _stowedMessages!)
        {
            logger.Debug($"STOWED: {message}");
        }
    }

    internal bool HasMessages => _stowedMessages!.Count > 0;

    internal IMessage? Head
    {
        get
        {
            if (!HasMessages)
            {
                Reset();
                return null;
            }

            return _stowedMessages!.Dequeue();
        }
    }

    internal void Reset()
    {
        _stowedMessages = new Queue<IMessage>();
        _stowing = false;
        _dispersing = false;
    }

    internal bool IsStowing => _stowing;

    internal void StowingMode()
    {
        _stowing = true;
        _dispersing = false;
    }

    internal bool IsDispersing => _dispersing;

    internal void DispersingMode()
    {
        _stowing = false;
        _dispersing = true;
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
            IMessage toStow;
            if (message.IsStowed)
            {
                toStow = message;
            }
            else
            {
                var closedMsgType = message.GetType().GetGenericArguments().First();
                var stowedLocalMsgType = typeof(StowedLocalMessage<>).MakeGenericType(closedMsgType);
                toStow = (IMessage)Activator.CreateInstance(stowedLocalMsgType, message)!;
            }

            _stowedMessages!.Enqueue(toStow);
        }
    }

    internal IMessage SwapWith<T>(IMessage newerMessage)
    {
        if (!HasMessages)
        {
            Reset();
            return newerMessage;
        }

        var olderMessage = _stowedMessages!.Dequeue();
        _stowedMessages.Enqueue(new StowedLocalMessage<T>((LocalMessage<T>)newerMessage));
        return olderMessage;
    }
}