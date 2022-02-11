// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.Plugin.Supervision;

public class CommonSupervisorsPluginConfiguration : IPluginConfiguration
{
    private CommonSupervisorsPluginConfiguration()
    {
        Supervisors = new List<ConfiguredSupervisor>();
    }

    public static CommonSupervisorsPluginConfiguration Define() => new CommonSupervisorsPluginConfiguration();

    public CommonSupervisorsPluginConfiguration WithSupervisor(
        string stageName,
        string supervisorName,
        Type supervisedProtocol,
        Type supervisorClass)
    {
        Supervisors.Add(new ConfiguredSupervisor(stageName, supervisorName, supervisedProtocol, supervisorClass));
        return this;
    }

    public int Count => Supervisors.Count;

    public string SupervisorName(int index) => Supervisors[index].SupervisorName;

    public string StageName(int index) => Supervisors[index].StageName;

    public Type? SupervisedProtocol(int index) => Supervisors[index].SupervisedProtocol;

    public Type? SupervisorClass(int index) => Supervisors[index].SupervisorClass;

    public string Name => SupervisorName(0);

    internal IList<ConfiguredSupervisor> Supervisors { get; }

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

            if (Supervisors.Contains(supervisor))
            {
                Supervisors.Remove(supervisor);
            }
            Supervisors.Add(supervisor);
        }
        configuration.With(this);
    }
}