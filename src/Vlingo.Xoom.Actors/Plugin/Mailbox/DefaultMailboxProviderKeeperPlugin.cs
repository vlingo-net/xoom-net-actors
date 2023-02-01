// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Mailbox;

public class DefaultMailboxProviderKeeperPlugin : IPlugin
{
    private readonly IMailboxProviderKeeper _keeper;
    private readonly DefaultMailboxProviderKeeperPluginConfiguration _configuration;

    public DefaultMailboxProviderKeeperPlugin(
        IMailboxProviderKeeper keeper,
        DefaultMailboxProviderKeeperPluginConfiguration configuration)
    {
        _keeper = keeper;
        _configuration = configuration;
    }

    private DefaultMailboxProviderKeeperPlugin(IPluginConfiguration configuration, DefaultMailboxProviderKeeperPlugin plugin)
    {
        _keeper = plugin._keeper;
        _configuration = (DefaultMailboxProviderKeeperPluginConfiguration)configuration;
    }

    public string Name => _configuration.Name;

    public int Pass => 0;

    public IPluginConfiguration Configuration => _configuration;

    public void Close()
    {
    }

    public void Start(IRegistrar registrar) => registrar.RegisterMailboxProviderKeeper(_keeper);

    public IPlugin With(IPluginConfiguration? overrideConfiguration)
        => overrideConfiguration == null ? this : new DefaultMailboxProviderKeeperPlugin(overrideConfiguration, this);
}