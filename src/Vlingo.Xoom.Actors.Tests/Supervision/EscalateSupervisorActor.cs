// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Tests.Supervision
{
    public class EscalateSupervisorActor : Actor, ISupervisor
    {
        private readonly EscalateSupervisorTestResults _testResults;

        public EscalateSupervisorActor(EscalateSupervisorTestResults testResults) => _testResults = testResults;

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            supervised.Escalate();
            _testResults.Access.WriteUsing("informedCount", 1);
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
            public AccessSafely Access { get; private set; }

            public EscalateSupervisorTestResults()
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
