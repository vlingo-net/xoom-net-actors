// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Xoom.Actors
{
    public sealed class Backoff
    {
        private const long BackoffCap = 4096;
        private const long BackoffReset = 0L;
        private const long BackoffStart = 1L;

        private long _backoff;
        private readonly bool _isFixed;

        public Backoff()
        {
            _backoff = BackoffReset;
            _isFixed = false;
        }

        public Backoff(long fixedBackoff)
        {
            _backoff = fixedBackoff;
            _isFixed = true;
        }

        public async Task Now(CancellationToken token)
        {
            if (!_isFixed)
            {
                if (_backoff == BackoffReset)
                {
                    _backoff = BackoffStart;
                }
                else if (_backoff < BackoffCap)
                {
                    _backoff = _backoff * 2;
                }
            }
            await YieldFor(_backoff, token);
        }

        public async Task Now()
        {
            await Now(CancellationToken.None);
        }

        public void Reset() => _backoff = BackoffReset;

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
