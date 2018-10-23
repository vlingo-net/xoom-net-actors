// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common;

namespace Vlingo.Actors.Tests.Supervision
{
    public class StopAllSupervisorActor : Actor, ISupervisor
    {
        public static readonly ThreadLocal<StopAllSupervisorActor> Instance = new ThreadLocal<StopAllSupervisorActor>();
        public AtomicInteger InformedCount { get; }

        public StopAllSupervisorActor()
        {
            Instance.Value = this;
            InformedCount = new AtomicInteger(0);
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            InformedCount.IncrementAndGet();
            supervised.Stop(SupervisionStrategy.Scope);
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => 5;

            public long Period => 1000;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.All;
        }
    }
}
