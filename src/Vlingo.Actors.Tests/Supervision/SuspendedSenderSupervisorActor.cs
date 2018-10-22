// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
    public class SuspendedSenderSupervisorActor : Actor, ISupervisor, IFailureControlSender
    {
        public static readonly ThreadLocal<SuspendedSenderSupervisorActor> Instance = new ThreadLocal<SuspendedSenderSupervisorActor>();

        private IFailureControl failureControl;
        private int times;

        public AtomicInteger InformedCount { get; }
        public TestUntil UntilInformed { get; set; }

        public SuspendedSenderSupervisorActor()
        {
            Instance.Value = this;
            InformedCount = new AtomicInteger(0);
        }

        public ISupervisionStrategy SupervisionStrategy { get; } = new SupervisionStrategyImpl();

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();


        public void Inform(Exception error, ISupervised supervised)
        {
            InformedCount.IncrementAndGet();
            for(var idx=1; idx <= times; ++idx)
            {
                failureControl.AfterFailureCount(idx);
            }
            supervised.Resume();
            UntilInformed.Happened();
        }

        public void SendUsing(IFailureControl failureControl, int times)
        {
            this.failureControl = failureControl;
            this.times = times;
        }

        private class SupervisionStrategyImpl : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
