// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Supervision
{
    public class CommonSupervisorsPlugin : AbstractPlugin
    {
        private readonly CommonSupervisorsPluginConfiguration configuration;

        public CommonSupervisorsPlugin()
        {
            configuration = CommonSupervisorsPluginConfiguration.Define();
        }

        private CommonSupervisorsPlugin(IPluginConfiguration configuration)
        {
            this.configuration = (CommonSupervisorsPluginConfiguration)configuration;
        }

        public override string Name => "common_supervisors";

        public override int Pass => 2;

        public override IPluginConfiguration Configuration => configuration;

        public override void Close()
        {
        }

        public override void Start(IRegistrar registrar)
        {
            foreach(var supervisor in configuration.Supervisors)
            {
                registrar.RegisterCommonSupervisor(
                    supervisor.StageName,
                    supervisor.SupervisorName,
                    supervisor.SupervisedProtocol,
                    supervisor.SupervisorClass);
            }
        }

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new CommonSupervisorsPlugin(overrideConfiguration);
    }
}
