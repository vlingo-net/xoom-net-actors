// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class RingBufferDispatcherPool
    {
        private readonly RingBufferDispatcher[] dispatchers;

        internal RingBufferDispatcherPool(
            int availableThreads,
            float numberOfDispatchersFactor,
            int ringBufferSize,
            long fixedBackoff,
            int throttlingCount)
        {
            var numberOfDispatchers = (int)(availableThreads * numberOfDispatchersFactor);
            dispatchers = new RingBufferDispatcher[numberOfDispatchers];
            for (int idx = 0; idx < dispatchers.Length; ++idx)
            {
                var dispatcher = new RingBufferDispatcher(ringBufferSize, fixedBackoff, throttlingCount);
                dispatcher.Start();
                dispatchers[idx] = dispatcher;
            }
        }

        internal RingBufferDispatcher AssignFor(int hashCode)
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
