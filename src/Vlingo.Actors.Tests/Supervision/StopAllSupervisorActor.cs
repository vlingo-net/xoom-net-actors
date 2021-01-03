// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors.Tests.Supervision
{
    public class StopAllSupervisorActor : Actor, ISupervisor
    {
        public static readonly ThreadLocal<StopAllSupervisorActor> Instance = new ThreadLocal<StopAllSupervisorActor>();
        private readonly StopAllSupervisorResult result;

        public StopAllSupervisorActor(StopAllSupervisorResult result)
        {
            Instance.Value = this;
            this.result = result;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            supervised.Stop(SupervisionStrategy.Scope);
            result.Access.WriteUsing("informedCount", 1);
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => 5;

            public long Period => 1000;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.All;
        }

        public class StopAllSupervisorResult
        {
            public AtomicInteger InformedCount { get; set; } = new AtomicInteger(0);
            public AccessSafely Access { get; private set; }
            public StopAllSupervisorResult()
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
