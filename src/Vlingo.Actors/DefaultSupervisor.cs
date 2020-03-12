// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public abstract class DefaultSupervisor : Actor, ISupervisor
    {
        internal static readonly ISupervisionStrategy DefaultSupervisionStrategy = new DefaultSupervisionStrategyImpl();

        internal DefaultSupervisor() { }

        public ISupervisionStrategy SupervisionStrategy => DefaultSupervisionStrategy;

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            Logger.Error(
                $"DefaultSupervisor: Failure of: {supervised.Address} because: {error.Message} Action: Possibly restarting.", 
                error);

            supervised.RestartWithin(
                DefaultSupervisionStrategy.Period,
                DefaultSupervisionStrategy.Intensity,
                DefaultSupervisionStrategy.Scope);
        }
    }
}
