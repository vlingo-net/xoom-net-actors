// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public interface ISupervisor
    {
        void Inform(Exception error, ISupervised supervised);
        ISupervisionStrategy SupervisionStrategy { get; }
        ISupervisor Supervisor { get; }
    }

    internal sealed class DefaultSupervisorImpl : ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy => DefaultSupervisor.DefaultSupervisionStrategy;

        public ISupervisor Supervisor => new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            var strategy = DefaultSupervisor.DefaultSupervisionStrategy;
            supervised.RestartWithin(strategy.Period, strategy.Intensity, strategy.Scope);
        }
    }

}