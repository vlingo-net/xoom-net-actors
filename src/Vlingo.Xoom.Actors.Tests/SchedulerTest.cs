// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class SchedulerTest : ActorsTest
    {
        private readonly IScheduled<CounterHolder> _scheduled;
        private readonly Scheduler _scheduler;

        public SchedulerTest()
        {
            _scheduled = new Scheduled();
            _scheduler = new Scheduler();
        }

        public override void Dispose()
        {
            _scheduler.Close();
            base.Dispose();
        }

        [Fact]
        public void TestScheduleOnceOneHappyDelivery()
        {
            var holder = new CounterHolder(1);

            _scheduler.ScheduleOnce(_scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

            Assert.Equal(1, holder.GetCounter());
        }

        [Fact]
        public void TestScheduleManyHappyDelivery()
        {
            var holder = new CounterHolder(505);

            _scheduler.Schedule(_scheduled, holder, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

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

        // TODO: This implementation is wrong, and needs to be changed.
        private class OnceScheduled : Actor, IFinalCountQuery, IScheduled<int>
        {
            private ICompletesEventually _completesEventually;
            private int _count;
            private readonly int _maximum;
            private readonly IScheduled<int> _scheduled;

            public OnceScheduled(int maximum)
            {
                _maximum = maximum;
                _count = 0;
                _scheduled = SelfAs<IScheduled<int>>();
            }

            public ICompletes<int> QueryCount()
            {
                _completesEventually = CompletesEventually();
                return (ICompletes<int>)Completes();
            }

            public void IntervalSignal(IScheduled<int> scheduled, int data)
            {
                if (_count < _maximum)
                {
                    Schedule();
                }
                else
                {
                    _completesEventually.With(_count);

                    SelfAs<IStoppable>().Stop();
                }
            }

            public override void Start()
            {
                Schedule();
            }

            private void Schedule()
            {
                ++_count;
                Scheduler.ScheduleOnce(_scheduled, _count, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1));
            }
        }

        private class CounterHolder
        {
            private readonly AccessSafely _safely;

            public CounterHolder(int times)
            {
                var counter = new AtomicInteger(0);
                _safely = AccessSafely
                    .AfterCompleting(times)
                    .WritingWith<int>("counter", _ => counter.IncrementAndGet())
                    .ReadingWith("counter", counter.Get);
            }

            public void Increment() => _safely.WriteUsing("counter", 1);

            public int GetCounter() => _safely.ReadFrom<int>("counter");
        }
    }

    public interface IFinalCountQuery
    {
        ICompletes<int> QueryCount();
    }
}
