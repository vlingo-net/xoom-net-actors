// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Eviction;

public class DirectoryEvictionPlugin : AbstractPlugin
{
    private readonly DirectoryEvictionConfiguration _configuration;

    public DirectoryEvictionPlugin(string? name = null) : this(DirectoryEvictionConfiguration.Define())
    {
    }

    private DirectoryEvictionPlugin(DirectoryEvictionConfiguration configuration) => _configuration = configuration;

    public override void Close()
    {
    }

    public override void Start(IRegistrar registrar)
    {
    }

    public override IPlugin With(IPluginConfiguration? overrideConfiguration)
    {
        if (overrideConfiguration == null)
        {
            return this;
        }
            
        return new DirectoryEvictionPlugin((DirectoryEvictionConfiguration) overrideConfiguration);
    }

    public override string Name => _configuration.Name;
    public override int Pass { get; } = 2;
    public override IPluginConfiguration Configuration => _configuration;
}