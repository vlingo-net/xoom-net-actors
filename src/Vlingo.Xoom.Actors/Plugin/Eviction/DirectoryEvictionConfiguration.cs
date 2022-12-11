// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Eviction;

public class DirectoryEvictionConfiguration : IPluginConfiguration
{
    private const long DefaultLruProbeInterval = 30 * 1_000L;   // 30 seconds
    private const long DefaultLruThreshold = 2 * 60 * 1_000;    // 2 minutes
    private const float DefaultFullRatioHighMark = 0.8F;        // 80%
        
    public static DirectoryEvictionConfiguration Define() => new DirectoryEvictionConfiguration();

    public string Name { get; private set; }
    public bool IsEnabled { get; private set; }
    public long LruProbeInterval { get; private set; }
    public long LruThreshold { get; private set; }
    public float FullRatioHighMark { get; private set; }

    public DirectoryEvictionConfiguration() : this(false, DefaultLruProbeInterval, DefaultLruThreshold, DefaultFullRatioHighMark)
    {
    }
        
    public DirectoryEvictionConfiguration(bool enabled, long lruProbeInterval, long lruThreshold, float fullRatioHighMark)
    {
        IsEnabled = enabled;
        LruProbeInterval = lruProbeInterval;
        LruThreshold = lruThreshold;
        FullRatioHighMark = fullRatioHighMark;
        Name = "directoryEviction";
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
        .With(WithLruThreshold(DefaultLruProbeInterval)
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
}