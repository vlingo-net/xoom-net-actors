// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Completes;

public class PooledCompletesPlugin : AbstractPlugin
{
    private readonly PooledCompletesPluginConfiguration _pooledCompletesPluginConfiguration;

    private ICompletesEventuallyProvider? _completesEventuallyProvider;

    public PooledCompletesPlugin(string? name = null) => _pooledCompletesPluginConfiguration = PooledCompletesPluginConfiguration.Define();

    private PooledCompletesPlugin(IPluginConfiguration configuration) => _pooledCompletesPluginConfiguration = (PooledCompletesPluginConfiguration)configuration;

    public override string Name => Configuration.Name;

    public override int Pass => 2;

    public override IPluginConfiguration Configuration => _pooledCompletesPluginConfiguration;

    public override void Close() => _completesEventuallyProvider!.Close();

    public override void Start(IRegistrar registrar)
    {
        _completesEventuallyProvider = new CompletesEventuallyPool(_pooledCompletesPluginConfiguration.PoolSize, _pooledCompletesPluginConfiguration.Mailbox!);
        registrar.Register(_pooledCompletesPluginConfiguration.Name, _completesEventuallyProvider!);
    }

    public override IPlugin With(IPluginConfiguration? overrideConfiguration)
        => overrideConfiguration == null ? this : new PooledCompletesPlugin(overrideConfiguration);
}