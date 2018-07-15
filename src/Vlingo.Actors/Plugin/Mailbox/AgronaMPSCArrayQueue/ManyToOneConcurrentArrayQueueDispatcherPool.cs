// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    internal class ManyToOneConcurrentArrayQueueDispatcherPool
    {
        private readonly ManyToOneConcurrentArrayQueueDispatcher[] dispatchers;

        internal ManyToOneConcurrentArrayQueueDispatcherPool(
            int availableThreads,
            float numberOfDispatchersFactor,
            int ringBufferSize,
            long fixedBackoff,
            int throttlingCount,
            int totalSendRetries)
        {
            var numberOfDispatchers = (int)(availableThreads * numberOfDispatchersFactor);
            dispatchers = new ManyToOneConcurrentArrayQueueDispatcher[numberOfDispatchers];

            for (int idx = 0; idx < dispatchers.Length; ++idx)
            {
                var dispatcher =
                        new ManyToOneConcurrentArrayQueueDispatcher(
                                ringBufferSize,
                                fixedBackoff,
                                throttlingCount,
                                totalSendRetries);

                dispatcher.Start();
                dispatchers[idx] = dispatcher;
            }
        }

        internal ManyToOneConcurrentArrayQueueDispatcher AssignFor(int hashCode)
        {
            var index = Math.Abs(hashCode) % dispatchers.Length;
            return dispatchers[index];
        }

        internal void Close()
        {
            for (int idx = 0; idx < dispatchers.Length; ++idx)
            {
                dispatchers[idx].Close();
            }
        }
    }
}
