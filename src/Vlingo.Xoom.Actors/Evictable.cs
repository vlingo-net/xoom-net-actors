// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors;

sealed class Evictable
{
    private readonly Actor _actor;
    private long _activeOn;

    internal Evictable(Actor actor)
    {
        _actor = actor;
        _activeOn = DateTimeHelper.CurrentTimeMillis();
    }
        
    internal void ReceivedMessage() => ActiveOn(DateTimeHelper.CurrentTimeMillis());

    void ActiveOn(long activeOn) => _activeOn = activeOn;

    internal bool Stop(long thresholdMillis) => Stop(DateTimeHelper.CurrentTimeMillis(), thresholdMillis);

    bool Stop(long referenceMillis, long thresholdMillis)
    {
        if (!_actor.Definition.Evictable)
        {
            return false;
        }

        var pendingMessageCount = _actor.LifeCycle.Environment.Mailbox.PendingMessages;
        if (IsStale(referenceMillis, thresholdMillis))
        {
            if (pendingMessageCount == 0)
            {
                _actor.SelfAs<IStoppable>().Stop();
                return true;
            }

            _actor.Logger.Warn(
                "Inactive Actor at {} failed to evict because it has {} undelivered messages in its mailbox",
                _actor.Address, pendingMessageCount);
        }
        return false;
    }

    internal bool IsStale(long thresholdMillis) => IsStale(DateTimeHelper.CurrentTimeMillis(), thresholdMillis);

    internal bool IsStale(long referenceMillis, long thresholdMillis) => _activeOn < referenceMillis - thresholdMillis;
}