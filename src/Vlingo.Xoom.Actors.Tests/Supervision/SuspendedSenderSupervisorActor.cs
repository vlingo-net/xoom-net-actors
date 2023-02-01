// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors.Tests.Supervision
{
    public class SuspendedSenderSupervisorActor : Actor, ISupervisor, IFailureControlSender
    {
        public static readonly ThreadLocal<SuspendedSenderSupervisorActor> Instance = new ThreadLocal<SuspendedSenderSupervisorActor>();
        private readonly SuspendedSenderSupervisorResults _results;
        private IFailureControl _failureControl;
        private int _times;

        public SuspendedSenderSupervisorActor(SuspendedSenderSupervisorResults results)
        {
            Instance.Value = this;
            _results = results;
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();


        public void Inform(Exception error, ISupervised supervised)
        {
            for (var idx = 1; idx <= _times; ++idx)
            {
                _failureControl.AfterFailureCount(idx);
            }
            supervised.Resume();
            _results.Access.WriteUsing("informedCount", 1);
        }

        public void SendUsing(IFailureControl failureControl, int times)
        {
            _failureControl = failureControl;
            _times = times;
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }

        public class SuspendedSenderSupervisorResults
        {
            public AccessSafely Access { get; private set; }
            public AtomicInteger InformedCount = new AtomicInteger(0);

            public SuspendedSenderSupervisorResults()
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
