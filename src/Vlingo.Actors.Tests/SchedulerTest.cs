// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class SchedulerTest : ActorsTest
    {
        private readonly IScheduled<CounterHolder> scheduled;
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
            var holder = new CounterHolder(1);

            scheduler.ScheduleOnce(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            Assert.Equal(1, holder.GetCounter());
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            var holder = new CounterHolder(505);

            scheduler.Schedule(scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            Assert.True(holder.GetCounter() > 500);
        }

        [Fact]
        public void TestThatManyScheduleOnceDeliver()
        {
            var query = World.ActorFor<IFinalCountQuery>(typeof(OnceScheduled), 10);
            var count = query.QueryCount().Await<int>();
            Assert.Equal(10, count);
        }


        private class Scheduled : IScheduled<CounterHolder>
        {
            public void IntervalSignal(IScheduled<CounterHolder> scheduled, CounterHolder data)
            {
                data.Increment();
            }
        }

        private class OnceScheduled : Actor, IFinalCountQuery, IScheduled<int>
        {
            private ICompletesEventually completesEventually;
            private int count;
            private readonly int maximum;
            private readonly IScheduled<int> scheduled;

            public OnceScheduled(int maximum)
            {
                this.maximum = maximum;
                count = 0;
                scheduled = SelfAs<IScheduled<int>>();
            }

            public ICompletes<int> QueryCount()
            {
                completesEventually = CompletesEventually();
                return (ICompletes<int>)Completes();
            }

            public void IntervalSignal(IScheduled<int> scheduled, int data)
            {
                if (count < maximum)
                {
                    Schedule();
                }
                else
                {
                    completesEventually.With(count);

                    SelfAs<IStoppable>().Stop();
                }
            }

            public override void Start()
            {
                Schedule();
            }

            private void Schedule()
            {
                ++count;
                Scheduler.ScheduleOnce(scheduled, count, TimeSpan.FromMilliseconds(count == 1 ? 1000 : 10), TimeSpan.FromMilliseconds(1));
            }
        }

        private class CounterHolder
        {
            private readonly AccessSafely safely;

            public CounterHolder(int times)
            {
                var counter = new AtomicInteger(0);
                safely = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith<int>("counter", _ => counter.IncrementAndGet())
                    .ReadingWith("counter", counter.Get);
            }

            public void Increment() => safely.WriteUsing("counter", 1);

            public int GetCounter() => safely.ReadFrom<int>("counter");
        }
    }

    public interface IFinalCountQuery
    {
        ICompletes<int> QueryCount();
    }
}
