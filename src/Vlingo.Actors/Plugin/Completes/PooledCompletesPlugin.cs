// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Completes
{
    public class PooledCompletesPlugin : AbstractPlugin
    {
        private readonly PooledCompletesPluginConfiguration pooledCompletesPluginConfiguration;

        private ICompletesEventuallyProvider? completesEventuallyProvider;

        public PooledCompletesPlugin()
        {
            pooledCompletesPluginConfiguration = PooledCompletesPluginConfiguration.Define();
        }

        private PooledCompletesPlugin(IPluginConfiguration configuration)
        {
            pooledCompletesPluginConfiguration = (PooledCompletesPluginConfiguration)configuration;
        }

        public override string Name => Configuration.Name;

        public override int Pass => 2;

        public override IPluginConfiguration Configuration => pooledCompletesPluginConfiguration;

        public override void Close()
        {
            completesEventuallyProvider!.Close();
        }

        public override void Start(IRegistrar registrar)
        {
            completesEventuallyProvider = new CompletesEventuallyPool(pooledCompletesPluginConfiguration.PoolSize, pooledCompletesPluginConfiguration.Mailbox!);
            registrar.Register(pooledCompletesPluginConfiguration.Name, completesEventuallyProvider!);
        }

        public override IPlugin With(IPluginConfiguration overrideConfiguration)
            => overrideConfiguration == null ? this : new PooledCompletesPlugin(overrideConfiguration);
    }
}
