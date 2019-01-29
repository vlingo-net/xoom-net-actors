// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors.Tests.Supervision
{
    public class EscalateSupervisorActor : Actor, ISupervisor
    {
        private readonly EscalateSupervisorTestResults testResults;

        public EscalateSupervisorActor(EscalateSupervisorTestResults testResults)
        {
            this.testResults = testResults;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            testResults.InformedCount.IncrementAndGet();
            supervised.Escalate();
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => 5;

            public long Period => 1000;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }

        public class EscalateSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; } = new AtomicInteger(0);
        }
    }
}
