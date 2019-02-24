// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;
using System;

namespace Vlingo.Actors.Tests
{
    public class SchedulerTest : ActorsTest
    {
        private readonly IScheduled scheduled;
        private readonly Scheduler scheduler;

        public SchedulerTest()
        {
            scheduled = new Scheduled();
            scheduler = new Scheduler();
        }

        public override void Dispose()
        {
            scheduler.Close();
            base.Dispose();
        }

        [Fact]
        public void TestScheduleOnceOneHappyDelivery()
        {
            var holder = new CounterHolder();
            holder.Until = Until(1);

            scheduler.ScheduleOnce(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
            holder.Until.Completes();

            Assert.Equal(1, holder.Counter);
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            var holder = new CounterHolder();
            holder.Until = Until(505);

            scheduler.Schedule(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
            holder.Until.Completes();

            Assert.True(holder.Counter > 500);
        }


        private class Scheduled : IScheduled
        {
            public void IntervalSignal(IScheduled scheduled, object data)
            {
                ((CounterHolder)data).Increment();
            }
        }

        private class CounterHolder
        {
            public int Counter { get; private set; } = 0;
            public TestUntil Until { get; set; }

            public void Increment()
            {
                ++Counter;
                Until.Happened();
            }
        }
    }
}
