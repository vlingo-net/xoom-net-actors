// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Actors.Plugin.Eviction;

public class DirectoryEvictionConfiguration : IPluginConfiguration
{
    internal const long DefaultLruProbeInterval = 30 * 1_000L;   // 30 seconds
    internal const long DefaultLruThreshold = 2 * 60 * 1_000;    // 2 minutes
    internal const float DefaultFullRatioHighMark = 0.8F;        // 80%
        
    public static DirectoryEvictionConfiguration Define() => new DirectoryEvictionConfiguration();

    public string Name { get; private set; }
    public bool IsEnabled { get; private set; }
    public IReadOnlyList<string> ExcludedStageNames { get; private set; }
    public long LruProbeInterval { get; private set; }
    public long LruThreshold { get; private set; }
    public float FullRatioHighMark { get; private set; }

    public DirectoryEvictionConfiguration() : this(
        false,
        new List<string>(),
        DefaultLruProbeInterval,
        DefaultLruThreshold,
        DefaultFullRatioHighMark)
    {
    }
        
    public DirectoryEvictionConfiguration(
        bool enabled,
        IReadOnlyList<string>? excludedStageNames,
        long lruProbeInterval,
        long lruThreshold,
        float fullRatioHighMark)
    {
        IsEnabled = enabled;
        ExcludedStageNames = excludedStageNames ?? new List<string>();
        LruProbeInterval = lruProbeInterval;
        LruThreshold = lruThreshold;
        FullRatioHighMark = fullRatioHighMark;
        Name = "directoryEviction";
    }

    public bool IsExcluded(Stage stage) => ExcludedStageNames.Contains(stage.Name);
    
    public DirectoryEvictionConfiguration Exclude(List<string>? excludedStageNames)
    {
        ExcludedStageNames = excludedStageNames ?? new List<string>();
        return this;
    }
    
    public DirectoryEvictionConfiguration WithEnabled(bool enabled)
    {
        IsEnabled = enabled;
        return this;
    }
        
    public DirectoryEvictionConfiguration WithLruProbInterval(long millis)
    {
        LruProbeInterval = millis;
        return this;
    }
        
    public DirectoryEvictionConfiguration WithLruThreshold(long millis)
    {
        LruThreshold = millis;
        return this;
    }
    
    public DirectoryEvictionConfiguration WithFullRatioHighMark(float ratio)
    {
        FullRatioHighMark = ratio;
        return this;
    }

    public void Build(Configuration configuration) => configuration
        .With(Exclude(DefaultExcludes(Array.Empty<string>()))
            .WithLruProbInterval(DefaultLruProbeInterval)
            .WithLruThreshold(DefaultLruThreshold)
            .WithFullRatioHighMark(DefaultFullRatioHighMark));

    public void BuildWith(Configuration configuration, PluginProperties properties)
    {
        Name = properties.Name;
        IsEnabled = properties.GetBoolean("enabled", true);
        LruProbeInterval = properties.GetLong("lruProbeInterval", DefaultLruProbeInterval);
        LruThreshold = properties.GetLong("lruThreshold", DefaultLruThreshold);
        FullRatioHighMark = properties.GetFloat("fullRatioHighMark", DefaultFullRatioHighMark);
    }

    public override string ToString() =>
        $"DirectoryEvictionConfiguration(name='{Name}', enabled='{IsEnabled}', lruProbeInterval='{LruProbeInterval}', lruThreshold='{LruThreshold}', fullRatioHighMark='{FullRatioHighMark:F2}')";
    
    private List<string> DefaultExcludes(string[]? stageNames)
    {
        List<string> excluded;
    
        if (stageNames == null || stageNames.Length == 0)
        {
            excluded = new List<string> { World.DefaultStage };
        }
        else
        {
            excluded = new List<string>(stageNames.Length);
            excluded.AddRange(stageNames.Select(stageName => stageName.Trim()));
        }
    
        return excluded;
    }
}