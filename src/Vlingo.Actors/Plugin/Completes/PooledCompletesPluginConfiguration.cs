// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Completes
{
    public class PooledCompletesPluginConfiguration : IPluginConfiguration
    {
        private PooledCompletesPluginConfiguration()
        {
            Name = "pooledCompletes";
        }

        public static PooledCompletesPluginConfiguration Define() => new PooledCompletesPluginConfiguration();

        public PooledCompletesPluginConfiguration WithMailbox(string mailbox)
        {
            Mailbox = mailbox;
            return this;
        }

        public PooledCompletesPluginConfiguration WithPoolSize(int poolSize)
        {
            PoolSize = poolSize;
            return this;
        }

        public string Mailbox { get; private set; }

        public int PoolSize { get; private set; }
        public string Name { get; private set; }

        public void Build(Configuration configuration)
        {
            configuration.With(
                WithMailbox("queueMailbox")
                .WithPoolSize(10)
            );
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            Name = properties.Name;
            Mailbox = properties.GetString("mailbox", null);
            PoolSize = properties.GetInteger("pool", 10);
            configuration.With(this);
        }
    }
}
