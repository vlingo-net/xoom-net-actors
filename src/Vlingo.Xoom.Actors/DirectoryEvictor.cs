// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Diagnostics;
using System.Linq;
using Vlingo.Xoom.Actors.Plugin.Eviction;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors;

internal class DirectoryEvictor : Actor, IScheduled<object>
{
    private readonly DirectoryEvictionConfiguration _config;
    private readonly Directory _directory;
        
    public DirectoryEvictor(Directory directory) : this(new DirectoryEvictionConfiguration(), directory)
    {
    }

    public DirectoryEvictor(DirectoryEvictionConfiguration config, Directory directory)
    {
        _config = config;
        _directory = directory;
        Logger.Debug("Created with config: {}", config);
    }
        
    public void IntervalSignal(IScheduled<object> scheduled, object data)
    {
        Logger.Debug("Started eviction routine");
        var currentProcess = Process.GetCurrentProcess();
        var fillRatio = currentProcess.PrivateMemorySize64 / (float) currentProcess.WorkingSet64;
        if (fillRatio >= _config.FullRatioHighMark)
        {
            Logger.Debug($"Memory fill ratio {fillRatio} exceeding watermark ({_config.FullRatioHighMark})");
            var evicted = _directory.EvictionCandidates(_config.LruThreshold)
                .Where(actor => actor.LifeCycle.Evictable.Stop(_config.LruThreshold))
                .Select(actor => actor.Address)
                .ToArray();
            Logger.Debug($"Evicted {evicted.Length} actors :: {evicted}");
        }
        else 
        {
            Logger.Debug($"Memory fill ratio {fillRatio} was below watermark ({_config.FullRatioHighMark})");
        }
    }
}