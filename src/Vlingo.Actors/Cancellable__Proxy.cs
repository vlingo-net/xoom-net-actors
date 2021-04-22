// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Actors
{
    public class Cancellable__Proxy : ICancellable
    {
        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Cancellable__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }
        public bool Cancel()
        {
            if (!_actor.IsStopped)
            {
                Action<ICancellable> consumer = x => x.Cancel();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, null, "Cancel()");
                }
                else
                {
                    _mailbox.Send(new LocalMessage<ICancellable>(_actor, consumer, "Cancel()"));
                }
                
                return true;
            }

            _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, "Cancel()"));
            return false;
        }
    }
}
