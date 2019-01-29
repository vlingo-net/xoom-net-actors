// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Actors
{
    public sealed class Backoff
    {
        private const long BACKOFF_CAP = 4096;
        private const long BACKOFF_RESET = 0L;
        private const long BACKOFF_START = 1L;

        private long backoff;
        private readonly bool isFixed;

        public Backoff()
        {
            backoff = BACKOFF_RESET;
            isFixed = false;
        }

        public Backoff(long fixedBackoff)
        {
            backoff = fixedBackoff;
            isFixed = true;
        }

        public async Task Now(CancellationToken token)
        {
            if (!isFixed)
            {
                if (backoff == BACKOFF_RESET)
                {
                    backoff = BACKOFF_START;
                }
                else if (backoff < BACKOFF_CAP)
                {
                    backoff = backoff * 2;
                }
            }
            await YieldFor(backoff, token);
        }

        public async Task Now()
        {
            await Now(CancellationToken.None);
        }

        public void Reset()
        {
            backoff = BACKOFF_RESET;
        }

        private async Task YieldFor(long aMillis, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(aMillis), token);
            }
            catch (OperationCanceledException)
            {
                // TODO: should ne logged
            }
        }
    }
}
