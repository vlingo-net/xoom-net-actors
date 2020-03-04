// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Supervision
{
    public class DefaultSupervisorOverridePlugin : AbstractPlugin
    {
        private readonly DefaultSupervisorOverridePluginConfiguration configuration;

        public DefaultSupervisorOverridePlugin()
        {
            configuration = DefaultSupervisorOverridePluginConfiguration.Define();
        }

        private DefaultSupervisorOverridePlugin(IPluginConfiguration configuration)
        {
            this.configuration = (DefaultSupervisorOverridePluginConfiguration)configuration;
        }

        public override string Name => "override_supervisor";

        public override int Pass => 2;

        public override IPluginConfiguration Configuration => configuration;

        public override void Close()
        {
        }

        public override void Start(IRegistrar registrar)
        {
            foreach(var supervisor in configuration.Supervisors)
            {
                registrar.RegisterDefaultSupervisor(supervisor.StageName, supervisor.SupervisorName, supervisor.SupervisorClass);
            }
        }

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new DefaultSupervisorOverridePlugin(overrideConfiguration);
    }
}
