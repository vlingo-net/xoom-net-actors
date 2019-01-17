// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Supervision
{
    public sealed class DefaultSupervisorOverride : Actor, ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy { get; } = new DefaultSupervisorOvverideSupervisionStrategy();

        public ISupervisor Supervisor { get; private set; }

        public DefaultSupervisorOverride()
        {
            Supervisor = ParentAs<ISupervisor>();
        }

        public void Inform(Exception error, ISupervised supervised)
        {
            Logger.Log($"DefaultSupervisorOverride: Failure of: {supervised.Address}", error);
            supervised.Resume();
        }

        private class DefaultSupervisorOvverideSupervisionStrategy : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
