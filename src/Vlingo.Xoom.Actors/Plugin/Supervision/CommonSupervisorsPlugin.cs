// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Supervision
{
    public class CommonSupervisorsPlugin : AbstractPlugin
    {
        private readonly CommonSupervisorsPluginConfiguration _configuration;

        public CommonSupervisorsPlugin(string? name = null) => _configuration = CommonSupervisorsPluginConfiguration.Define();

        private CommonSupervisorsPlugin(IPluginConfiguration configuration) => _configuration = (CommonSupervisorsPluginConfiguration)configuration;

        public override string Name => "common_supervisors";

        public override int Pass => 2;

        public override IPluginConfiguration Configuration => _configuration;

        public override void Close()
        {
        }

        public override void Start(IRegistrar registrar)
        {
            foreach(var supervisor in _configuration.Supervisors)
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
