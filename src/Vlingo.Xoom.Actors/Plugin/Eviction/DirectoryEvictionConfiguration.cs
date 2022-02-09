// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Eviction
{
    public class DirectoryEvictionConfiguration : IPluginConfiguration
    {
        private const long DefaultLruMillis = 10 * 60 * 1_000L;
        private const float DefaultFillRatioHigh = 0.8F;
        
        public static DirectoryEvictionConfiguration Define() => new DirectoryEvictionConfiguration();

        public string Name { get; private set; }
        public bool IsEnabled { get; private set; }
        public long LruThresholdMillis { get; private set; }
        public float FillRatioHigh { get; private set; }

        public DirectoryEvictionConfiguration() : this(false, DefaultLruMillis, DefaultFillRatioHigh)
        {
        }
        
        public DirectoryEvictionConfiguration(bool enabled, long lruThresholdMillis, float fillRatioHigh)
        {
            IsEnabled = enabled;
            LruThresholdMillis = lruThresholdMillis;
            FillRatioHigh = fillRatioHigh;
            Name = "directoryEviction";
        }
        
        public DirectoryEvictionConfiguration WithEnabled(bool enabled)
        {
            IsEnabled = enabled;
            return this;
        }
        
        public DirectoryEvictionConfiguration WithLruThresholdMillis(long millis)
        {
            LruThresholdMillis = millis;
            return this;
        }
        
        public DirectoryEvictionConfiguration WithFillRatioHigh(float ratio)
        {
            FillRatioHigh = ratio;
            return this;
        }

        public void Build(Configuration configuration) => configuration.With(WithLruThresholdMillis(DefaultLruMillis).WithFillRatioHigh(DefaultFillRatioHigh));

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            Name = properties.Name;
            IsEnabled = properties.GetBoolean("enabled", true);
            LruThresholdMillis = properties.GetLong("lruThresholdMillis", DefaultLruMillis);
            FillRatioHigh = properties.GetFloat("fillRatioHigh", DefaultFillRatioHigh);
        }

        public override string ToString() =>
            $"DirectoryEvictionConfiguration(name='{Name}', enabled='{IsEnabled}', lruThresholdMillis='{LruThresholdMillis}', fillRatioHigh='{DefaultFillRatioHigh:F2}')";
    }
}