// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Common.Completion.Tasks;

namespace Vlingo.Actors
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
        public bool IsCompleted => throw new NotImplementedException();

        public bool HasFailed => throw new NotImplementedException();

        public void Failed(Exception exception) => throw new NotImplementedException();

        public bool HasOutcome => throw new NotImplementedException();

        public T Outcome => throw new NotImplementedException();

        public ResultCompletes()
            : this(null, null, false)
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

        public ICompletes<T> With(T outcome)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TimeSpan timeout, TO failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TO failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(TimeSpan timeout, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TO> AndThen<TO>(Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(TimeSpan timeout, Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> AndThenConsume(Action consumer)
        {
            throw new NotImplementedException();
        }

        public TNewResult AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, TNewResult failedOutcomeValue, Func<T, ICompletes<TNewResult>> function)
        {
            throw new NotImplementedException();
        }

        public TNewResult AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<T, TNewResult> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TNewResult failedOutcomeValue, Func<T, ICompletes<TNewResult>> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TF, TO>(TimeSpan timeout, TF failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TF, TO>(TF failedOutcomeValue, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TO>(TimeSpan timeout, Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(TimeSpan timeout, Func<T, ICompletes<TNewResult>> function)
        {
            throw new NotImplementedException();
        }

        public TO AndThenTo<TO>(Func<T, TO> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TNewResult> AndThenTo<TNewResult>(Func<T, ICompletes<TNewResult>> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<TFailedOutcome> Otherwise<TFailedOutcome>(Func<TFailedOutcome, TFailedOutcome> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> Otherwise(Func<T, T> function)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> OtherwiseConsume(Action<T> consumer)
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> RecoverFrom(Func<Exception, T> function)
        {
            throw new NotImplementedException();
        }

        public T Await()
        {
            throw new NotImplementedException();
        }

        public TO Await<TO>()
        {
            throw new NotImplementedException();
        }

        public T Await(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public TO Await<TO>(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void Failed()
        {
            throw new NotImplementedException();
        }

        public ICompletes<T> Repeat()
        {
            throw new NotImplementedException();
        }

        public CompletesAwaiter<T> GetAwaiter()
        {
            throw new NotImplementedException();
        }

        public void SetException(Exception exception)
        {
            throw new NotImplementedException();
        }

        public void SetResult(T result)
        {
            throw new NotImplementedException();
        }
    }
}
