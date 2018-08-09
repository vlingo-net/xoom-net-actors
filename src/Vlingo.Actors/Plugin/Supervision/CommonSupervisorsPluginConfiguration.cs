// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors.Plugin.Supervision
{
    public class CommonSupervisorsPluginConfiguration : IPluginConfiguration
    {
        private readonly IList<ConfiguredSupervisor> supervisors;

        private CommonSupervisorsPluginConfiguration()
        {
            supervisors = new List<ConfiguredSupervisor>();
        }

        public static CommonSupervisorsPluginConfiguration Define() => new CommonSupervisorsPluginConfiguration();

        public CommonSupervisorsPluginConfiguration WithSupervisor(
            string stageName,
            string supervisorName,
            Type supervisedProtocol,
            Type supervisorClass)
        {
            supervisors.Add(new ConfiguredSupervisor(stageName, supervisorName, supervisedProtocol, supervisorClass));
            return this;
        }

        public int Count => supervisors.Count;

        public string SupervisorName(int index) => supervisors[index].SupervisorName;

        public string StageName(int index) => supervisors[index].StageName;

        public Type SupervisedProtocol(int index) => supervisors[index].SupervisedProtocol;

        public Type SupervisorClass(int index) => supervisors[index].SupervisorClass;

        public string Name => SupervisorName(0);

        public void Build(Configuration configuration)
        {
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            foreach (var values in DefinitionValues.AllDefinitionValues(properties))
            {
                var supervisor = new ConfiguredSupervisor(
                    values.StageName,
                    values.Name,
                    values.Protocol,
                    values.Supervisor);

                if (supervisors.Contains(supervisor))
                {
                    supervisors.Remove(supervisor);
                }
                supervisors.Add(supervisor);
            }
            configuration.With(this);
        }
    }
}
