// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors.Plugin.Supervision
{
    public class DefaultSupervisorOverridePluginConfiguration : IPluginConfiguration
    {
        private DefaultSupervisorOverridePluginConfiguration()
        {
            Supervisors = new List<ConfiguredSupervisor>();
        }

        public static DefaultSupervisorOverridePluginConfiguration Define() => new DefaultSupervisorOverridePluginConfiguration();

        public DefaultSupervisorOverridePluginConfiguration WithSupervisor(
            string stageName,
            string supervisorName,
            Type? supervisorClass)
        {
            Supervisors.Add(new ConfiguredSupervisor(stageName, supervisorName, supervisorClass));
            return this;
        }

        public int Count => Supervisors.Count;

        public string SupervisorName(int index) => Supervisors[index].SupervisorName;

        public string StageName(int index) => Supervisors[index].StageName;

        public Type? SupervisorClass(int index) => Supervisors[index].SupervisorClass;

        internal IList<ConfiguredSupervisor> Supervisors { get; }

        public string Name => SupervisorName(0);

        public void Build(Configuration configuration)
        {
            configuration.With(WithSupervisor(
                "default",
                "overrideSupervisor",
                ConfiguredSupervisor.SupervisorFrom("Vlingo.Actors.Plugin.Supervision.DefaultSupervisorOverride")));
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            foreach (var values in DefinitionValues.AllDefinitionValues(properties))
            {
                var supervisor = new ConfiguredSupervisor(
                    values.StageName,
                    values.Name,
                    values.Supervisor);

                Supervisors.Add(supervisor);
            }
            configuration.With(this);
        }
    }
}
