// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics;
using System.Linq;
using Vlingo.Actors.Plugin.Eviction;
using Vlingo.Common;

namespace Vlingo.Actors
{
    internal class DirectoryEvictor : Actor, IScheduled<object>
    {
        private readonly DirectoryEvictionConfiguration config;
        private readonly Directory directory;
        
        public DirectoryEvictor(Directory directory) : this(new DirectoryEvictionConfiguration(), directory)
        {
        }

        public DirectoryEvictor(DirectoryEvictionConfiguration config, Directory directory)
        {
            this.config = config;
            this.directory = directory;
            Logger.Debug("Created with config: {}", config);
        }
        
        public void IntervalSignal(IScheduled<object> scheduled, object data)
        {
            Logger.Debug("Started eviction routine");
            var currentProcess = Process.GetCurrentProcess();
            var fillRatio = currentProcess.PrivateMemorySize64 / (float) currentProcess.WorkingSet64;
            if (fillRatio >= config.FillRatioHigh)
            {
                Logger.Debug($"Memory fill ratio {fillRatio} exceeding watermark ({config.FillRatioHigh})");
                var evicted = directory.EvictionCandidates(config.LruThresholdMillis)
                    .Where(actor => actor.LifeCycle.Evictable.Stop(config.LruThresholdMillis))
                    .Select(actor => actor.Address)
                    .ToArray();
                Logger.Debug($"Evicted {evicted.Length} actors :: {evicted}");
            }
            else 
            {
                Logger.Debug($"Memory fill ratio {fillRatio} was below watermark ({config.FillRatioHigh})");
            }
        }
    }
}