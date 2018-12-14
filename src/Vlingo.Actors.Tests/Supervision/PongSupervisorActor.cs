// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class PongSupervisorActor : Actor, ISupervisor
    {
        public static readonly ThreadLocal<PongSupervisorActor> Instance = new ThreadLocal<PongSupervisorActor>();

        internal readonly PongSupervisorTestResults TestResults;

        public PongSupervisorActor()
        {
            TestResults = new PongSupervisorTestResults();
            Instance.Value = this;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            TestResults.InformedCount.IncrementAndGet();
            supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
            TestResults.UntilInform.Happened();
        }

        internal class PongSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; set; } = new AtomicInteger(0);
            public TestUntil UntilInform { get; set; } = TestUntil.Happenings(0);
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => 10;

            public long Period => 1000;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
