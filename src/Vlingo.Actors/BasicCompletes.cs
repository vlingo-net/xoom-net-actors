// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Common;

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
            state = new State();
        }

        public ICompletes<T> After(Func<T> supplier) => After(supplier, -1L, default(T));

        public ICompletes<T> After(Func<T> supplier, long timeout) => After(supplier, timeout, default(T));

        public ICompletes<T> After(Func<T> supplier, long timeout, T timedOutValue)
        {
            state.timedOutValue = timedOutValue;
            state.actions.Enqueue(supplier);
            if (state.IsCompleted && outcome.Get() != null)
            {
                ExecuteActions();
            }
            else
            {
                StartTimer(timeout);
            }
            return this;
        }

        public ICompletes<T> After(Action<T> consumer) => After(consumer, -1L, default(T));

        public ICompletes<T> After(Action<T> consumer, long timeout) => After(consumer, timeout, default(T));

        public ICompletes<T> After(Action<T> consumer, long timeout, T timedOutValue)
        {
            state.timedOutValue = timedOutValue;
            state.actions.Enqueue(consumer);
            if (state.IsCompleted && outcome.Get() != null)
            {
                ExecuteActions();
            }
            else
            {
                StartTimer(timeout);
            }
            return this;
        }

        public ICompletes<T> AndThen(Action<T> consumer)
        {
            state.actions.Enqueue(consumer);
            if (state.IsCompleted && outcome.Get() != null)
            {
                ExecuteActions();
            }
            return this;
        }

        public ICompletes<T> AtLast(Func<T> supplier)
        {
            state.actions.Enqueue(supplier);
            if(state.IsCompleted && outcome.Get() != null)
            {
                ExecuteActions();
                outcome.Set(new OutcomeData(supplier.Invoke()));
            }
            return this;
        }

        public ICompletes<T> AtLast(Action<T> consumer)
        {
            state.actions.Enqueue(consumer);
            if(state.IsCompleted && outcome.Get() != null)
            {
                consumer.Invoke(outcome.Get().data);
            }
            return this;
        }

        public bool HasOutcome => outcome.Get() != null;

        public virtual T Outcome => outcome.Get().data;

        object ICompletes.Outcome => outcome.Get().data;

        public virtual ICompletes<TOutcome> With<TOutcome>(TOutcome outcome)
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

        public ICompletes With(object outcome) => With<object>(outcome);

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

                ExecuteActions();
            }
        }

        private void ExecuteActions()
        {
            while (!state.executingActions.CompareAndSet(false, true)) ;

            while (!state.actions.IsEmpty)
            {
                if(state.actions.TryDequeue(out object action))
                {
                    if(action is Func<T>)
                    {
                        outcome.Set(new OutcomeData((action as Func<T>).Invoke()));
                    }
                    else if(action is Action<T>)
                    {
                        (action as Action<T>).Invoke(outcome.Get().data);
                    }
                }
            }
            state.executingActions.Set(false);
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
            internal ConcurrentQueue<object> actions;
            internal ICancellable cancellable;
            internal AtomicBoolean completed;
            internal Scheduler scheduler;
            internal T timedOutValue;
            internal AtomicBoolean executingActions;
            internal State(Scheduler scheduler)
            {
                this.scheduler = scheduler;
                this.actions = new ConcurrentQueue<object>();
                this.completed = new AtomicBoolean(false);
                this.executingActions = new AtomicBoolean(false);
            }

            internal State() : this(null) { }

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
