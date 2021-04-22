// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Common;
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
            supervised.RestartWithin(SupervisionStrategy.Period, SupervisionStrategy.Intensity, SupervisionStrategy.Scope);
            TestResults.Access.WriteUsing("informedCount", 1);
        }

        internal class PongSupervisorTestResults
        {
            public AtomicInteger InformedCount { get; set; } = new AtomicInteger(0);
            public AccessSafely Access { get; private set; }

            public PongSupervisorTestResults()
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

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => 10;

            public long Period => 1000;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
