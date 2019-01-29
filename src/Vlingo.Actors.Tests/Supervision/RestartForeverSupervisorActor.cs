// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class RestartForeverSupervisorActor : Actor, ISupervisor
    {
        private readonly RestartForeverSupervisorTestResults testResults;

        public RestartForeverSupervisorActor(RestartForeverSupervisorTestResults testResults)
        {
            this.testResults = testResults;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            testResults.InformedCount.IncrementAndGet();
            supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
            testResults.UntilInform.Happened();
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }

        public class RestartForeverSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; set; } = new AtomicInteger(0);
            public TestUntil UntilInform { get; set; } = TestUntil.Happenings(0);
        }
    }
}
