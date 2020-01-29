// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Mailbox
{
    public class DefaultMailboxProviderKeeperPlugin : IPlugin
    {
        private readonly IMailboxProviderKeeper keeper;
        private readonly DefaultMailboxProviderKeeperPluginConfiguration configuration;

        public DefaultMailboxProviderKeeperPlugin(
            IMailboxProviderKeeper keeper,
            DefaultMailboxProviderKeeperPluginConfiguration configuration)
        {
            this.keeper = keeper;
            this.configuration = configuration;
        }

        private DefaultMailboxProviderKeeperPlugin(IPluginConfiguration configuration, DefaultMailboxProviderKeeperPlugin plugin)
        {
            keeper = plugin.keeper;
            this.configuration = (DefaultMailboxProviderKeeperPluginConfiguration)configuration;
        }

        public string Name => configuration.Name;

        public int Pass => 0;

        public IPluginConfiguration Configuration => configuration;

        public void Close()
        {
        }

        public void Start(IRegistrar registrar) => registrar.RegisterMailboxProviderKeeper(keeper);

        public IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new DefaultMailboxProviderKeeperPlugin(overrideConfiguration, this);
    }
}
