// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class BasicCompletes<T> : ICompletes<T>, IScheduled
    {
        private readonly AtomicReference<OutcomeData> outcome;
        private readonly State state;

        public BasicCompletes(Scheduler scheduler)
        {
            outcome = new AtomicReference<OutcomeData>(null);
            state = new State(scheduler);
        }

        public BasicCompletes(T outcome)
        {
            this.outcome = new AtomicReference<OutcomeData>(new OutcomeData(outcome));
        }

        public ICompletes<T> After(Func<T> supplier) => After(supplier, -1L, default(T));

        public ICompletes<T> After(Func<T> supplier, long timeout) => After(supplier, timeout, default(T));

        public ICompletes<T> After(Func<T> supplier, long timeout, T timedOutValue)
        {
            state.supplier = supplier;
            state.timedOutValue = timedOutValue;
            if (state.IsCompleted && outcome.Get() != null)
            {
                outcome.Set(new OutcomeData(state.supplier.Invoke()));
            }
            else
            {
                StartTimer(timeout);
            }
            return this;
        }

        public ICompletes<T> AndThen(Action<T> consumer)
        {
            state.andThen = consumer;
            if (state.IsCompleted && outcome.Get() != null)
            {
                state.andThen.Invoke(outcome.Get().data);
            }
            return this;
        }

        public ICompletes<T> After(Action<T> consumer) => After(consumer, -1L, default(T));

        public ICompletes<T> After(Action<T> consumer, long timeout) => After(consumer, timeout, default(T));

        public ICompletes<T> After(Action<T> consumer, long timeout, T timedOutValue)
        {
            state.consumer = consumer;
            state.timedOutValue = timedOutValue;
            if (state.IsCompleted && outcome.Get() != null)
            {
                state.consumer.Invoke(outcome.Get().data);
            }
            else
            {
                StartTimer(timeout);
            }
            return this;
        }

        public bool HasOutcome => outcome.Get() != null;

        public T Outcome => outcome.Get().data;

        public ICompletes<TOutcome> With<TOutcome>(TOutcome outcome)
        {
            if(state == null)
            {
                this.outcome.Set(new OutcomeData((T)(object)outcome));
            }
            else
            {
                CompletedWith(false, (T)(object)outcome);
            }

            return (ICompletes<TOutcome>)this;
        }

        public void IntervalSignal(IScheduled scheduled, object data)
        {
            CompletedWith(true, default(T));
        }

        internal BasicCompletes()
        {
            outcome = new AtomicReference<OutcomeData>(null);
            state = null;
        }

        internal void ClearOutcome()
        {
            outcome.Set(null);
        }

        private void CompletedWith(bool timedOut, T outcome)
        {
            if (state.completed.CompareAndSet(false, true))
            {
                this.outcome.Set(new OutcomeData(outcome));

                state.CancelTimer();

                if (timedOut)
                {
                    this.outcome.Set(new OutcomeData(state.timedOutValue));
                }

                if (state.supplier != null)
                {
                    this.outcome.Set(new OutcomeData(state.supplier.Invoke()));
                }

                if (state.consumer != null)
                {
                    state.consumer.Invoke(this.outcome.Get().data);
                }

                if (state.andThen != null)
                {
                    state.andThen.Invoke(this.outcome.Get().data);
                }
            }
        }

        private void StartTimer(long timeout)
        {
            if (timeout > 0)
            {
                // 2L delayBefore prevents timeout until after return from here
                state.cancellable = state.scheduler.ScheduleOnce(this, null, 2L, timeout);
            }
        }

        private class OutcomeData
        {
            internal T data;

            internal OutcomeData(T data)
            {
                this.data = data;
            }
        }

        private class State
        {
            internal Action<T> andThen;
            internal ICancellable cancellable;
            internal AtomicBoolean completed;
            internal Action<T> consumer;
            internal Scheduler scheduler;
            internal Func<T> supplier;
            internal T timedOutValue;
            internal State(Scheduler scheduler)
            {
                this.scheduler = scheduler;
                this.andThen = null;
                this.consumer = null;
                this.completed = new AtomicBoolean(false);
                this.supplier = null;
            }

            internal void CancelTimer()
            {
                if (cancellable != null)
                {
                    cancellable.Cancel();
                    cancellable = null;
                }
            }


            internal bool IsCompleted => completed.Get();
        }
    }
}
