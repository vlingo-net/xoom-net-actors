// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
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
    public class ResumeForeverSupervisorActor : Actor, ISupervisor
    {
        private readonly ResumeForeverSupervisorTestResults testResults;

        public ResumeForeverSupervisorActor(ResumeForeverSupervisorTestResults testResults)
        {
            this.testResults = testResults;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            testResults.Access.WriteUsing("informedCount", 1);
            if(testResults.Access.ReadFrom<int>("informedCount") == 1)
            {
                supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
            }
            else
            {
                supervised.Resume();
            }
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }

        public class ResumeForeverSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; set; } = new AtomicInteger(0);
            public AccessSafely Access { get; private set; }
            public ResumeForeverSupervisorTestResults()
            {
                Access = AfterCompleting(0);
            }

            public AccessSafely AfterCompleting(int times)
            {
                Access = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith("informedCount", (int increment) => InformedCount.Set(InformedCount.Get() + increment))
                    .ReadingWith("informedCount", () => InformedCount.Get());

                return Access;
            }
        }
    }
}
