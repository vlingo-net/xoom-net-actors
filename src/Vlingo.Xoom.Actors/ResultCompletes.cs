// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Common.Completion.Tasks;

namespace Vlingo.Xoom.Actors
{
    internal abstract class ResultCompletes : ICompletes
    {
        private ICompletes? _internalClientCompletes;
        internal ICompletes? InternalClientCompletes
        {
            get => ResultHolder!._internalClientCompletes;
            set => ResultHolder!._internalClientCompletes = value;
        }

        private object? _internalOutcome;
        internal object? InternalOutcome
        {
            get => ResultHolder!._internalOutcome;
            set => ResultHolder!._internalOutcome = value;
        }

        private bool _hasInternalOutcome;
        internal bool HasInternalOutcomeSet
        {
            get => ResultHolder!._hasInternalOutcome;
            set => ResultHolder!._hasInternalOutcome = value;
        }

        protected ResultCompletes? ResultHolder;

        public abstract ICompletes<O> With<O>(O outcome);
        public abstract ICompletes? ClientCompletes();
        public abstract void Reset(ICompletes? clientCompletes);
        public abstract bool IsOfSameGenericType<TOtherType>();
    }

    internal class ResultCompletes<T> : ResultCompletes, ICompletes<T>
    {
        private ICompletes<T> _completes = Common.Completes.Using<T>(new Scheduler());
        
        public bool IsCompleted => Completes().IsCompleted;

        public bool HasFailed => Completes().HasFailed;

        public void Failed(Exception exception) => Completes().Failed(exception);

        public bool HasOutcome => Completes().HasOutcome;

        public T Outcome => Completes().Outcome;

        public ResultCompletes() : this(null, null, false)
        {
        }

        private ResultCompletes(ICompletes? clientCompletes, object? internalOutcome, bool hasOutcomeSet)
        {
            ResultHolder = this;
            InternalClientCompletes = clientCompletes;
            InternalOutcome = internalOutcome;
            HasInternalOutcomeSet = hasOutcomeSet;
        }

        public override ICompletes<O> With<O>(O outcome)
        {
            HasInternalOutcomeSet = true;
            InternalOutcome = outcome;

            if (!ResultHolder!.IsOfSameGenericType<O>())
            {
                ResultHolder = new ResultCompletes<O>(InternalClientCompletes, InternalOutcome, HasInternalOutcomeSet);
            }

            return (ICompletes<O>)ResultHolder;
        }

        public override ICompletes? ClientCompletes()
        {
            return InternalClientCompletes;
        }

        public override void Reset(ICompletes? clientCompletes)
        {
           InternalClientCompletes = clientCompletes;
           InternalOutcome = null;
           HasInternalOutcomeSet = false;
        }

        public override bool IsOfSameGenericType<TOtherType>()
            => typeof(T) == typeof(TOtherType);

        public ICompletes<T> With(T outcome) => Completes().With(outcome);

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            _completes.AndThen(timeout, failedOutcomeValue, function);
            SetOutcome();
            return (ICompletes<TNewResult>)_completes;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            _completes.AndThen(failedOutcomeValue, function);
            SetOutcome();
            return (ICompletes<TNewResult>)_completes;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(TimeSpan timeout, Func<T, TNewResult> function)
        {
            _completes.AndThen(timeout, function);
            SetOutcome();
            return (ICompletes<TNewResult>)_completes;
        }

        public ICompletes<TNewResult> AndThen<TNewResult>(Func<T, TNewResult> function)
        {
            _completes.AndThen(function);
            SetOutcome();
            return (ICompletes<TNewResult>)_completes;
        }

        public ICompletes<T> AndThen(Func<T, T> function)
        {
            _completes.AndThen(function);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, Action<T> consumer)
        {
            _completes = Completes().AndThenConsume(timeout, failedOutcomeValue, consumer);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer)
        {
            _completes = Completes().AndThenConsume(failedOutcomeValue, consumer);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, Action<T> consumer)
        {
            _completes = Completes().AndThenConsume(timeout, consumer);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> AndThenConsume(Action<T> consumer)
        {
            _completes = Completes().AndThenConsume(consumer);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> AndThenConsume(Action consumer)
        {
            _completes = Completes().AndThenConsume(consumer);
            SetOutcome();
            return _completes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            var completes = Completes().AndThenTo(timeout, failedOutcomeValue, function);
            SetOutcome();
            return completes;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<T, ICompletes<TNewResult>> function)
        {
            _completes = (ICompletes<T>)Completes().AndThenTo(timeout, failedOutcomeValue, function);
            SetOutcome();
            return (ICompletes<TNewResult>) _completes;
        }

        public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            var completes = Completes().AndThenTo(failedOutcomeValue, function);
            SetOutcome();
            return completes;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<T, ICompletes<TNewResult>> function)
        {
            _completes = (ICompletes<T>)Completes().AndThenTo(failedOutcomeValue, function);
            SetOutcome();
            return (ICompletes<TNewResult>) _completes;
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, Func<T, TNewResult> function)
        {
            var completes = Completes().AndThenTo(timeout, function);
            SetOutcome();
            return completes;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<T, ICompletes<TNewResult>> function)
        {
            _completes = (ICompletes<T>)Completes().AndThenTo(timeout, function);
            SetOutcome();
            return (ICompletes<TNewResult>) _completes;
        }

        public TNewResult AndThenTo<TNewResult>(Func<T, TNewResult> function)
        {
            var completes = Completes().AndThenTo(function);
            SetOutcome();
            return completes;
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(Func<T, ICompletes<TNewResult>> function)
        {
            _completes = (ICompletes<T>)Completes().AndThenTo(function);
            SetOutcome();
            return (ICompletes<TNewResult>) _completes;
        }

        public ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, T> function)
        {
            var completes = Completes().Otherwise(function);
            SetOutcome();
            return completes;
        }

        public ICompletes<T> OtherwiseConsume(Action<T> consumer)
        {
            _completes = Completes().OtherwiseConsume(consumer);
            SetOutcome();
            return _completes;
        }

        public ICompletes<T> RecoverFrom(Func<Exception, T> function)
        {
            _completes = Completes().RecoverFrom(function);
            SetOutcome();
            return _completes;
        }

        public T Await() => Completes().Await();

        public TNewResult Await<TNewResult>() => Completes().Await<TNewResult>();

        public T Await(TimeSpan timeout) => Completes().Await(timeout);

        public TNewResult Await<TNewResult>(TimeSpan timeout) => Completes().Await<TNewResult>(timeout);

        public void Failed() => Completes().Failed();

        public ICompletes<T> Repeat() => Completes().Repeat();
        
        public ICompletes<T> TimeoutWithin(TimeSpan timeout) => Completes().TimeoutWithin(timeout);

        public ICompletes<T> UseFailedOutcomeOf(T failedOutcomeValue) => Completes().UseFailedOutcomeOf(failedOutcomeValue);

        public CompletesAwaiter<T> GetAwaiter() => new CompletesAwaiter<T>(this);

        public void SetException(Exception exception) => Completes().SetException(exception);

        public void SetResult(T result) => Completes().SetResult(result);
        
        public Task<T> ToTask() => ((BasicCompletes<T>)Completes()).ToTask();

        private ICompletes<T> Completes() => _completes;

        private void SetOutcome()
        {
            if (HasInternalOutcomeSet && !_completes.HasOutcome)
            {
                _completes.With(Outcome);
            }
        }
    }
}
