// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    internal abstract class ResultCompletes : ICompletes
    {
        private ICompletes internalClientCompletes;
        internal ICompletes InternalClientCompletes
        {
            get => resultHolder.internalClientCompletes;
            set => resultHolder.internalClientCompletes = value;
        }

        private object _internalOutcome;
        internal object InternalOutcome
        {
            get => resultHolder._internalOutcome;
            set => resultHolder._internalOutcome = value;
        }

        private bool _hasInternalOutcome;
        internal bool HasInternalOutcomeSet
        {
            get => resultHolder._hasInternalOutcome;
            set => resultHolder._hasInternalOutcome = value;
        }

        protected ResultCompletes resultHolder;

        public abstract ICompletes<O> With<O>(O outcome);
        public abstract ICompletes ClientCompletes();
        public abstract void Reset(ICompletes clientCompletes);
        public abstract bool IsOfSameGenericType<TOtherType>();
    }

    internal class ResultCompletes<T> : ResultCompletes, ICompletes<T>
    {
        public bool IsCompleted => throw new NotImplementedException();

        public bool HasFailed => throw new NotImplementedException();

        public bool HasOutcome => throw new NotImplementedException();

        public T Outcome => throw new NotImplementedException();

        public ResultCompletes()
            : this(null, null, false)
        {
        }

        private ResultCompletes(ICompletes clientCompletes, object internalOutcome, bool hasOutcomeSet)
        {
            resultHolder = this;
            InternalClientCompletes = clientCompletes;
            InternalOutcome = internalOutcome;
            HasInternalOutcomeSet = hasOutcomeSet;
        }

        public override ICompletes<O> With<O>(O outcome)
        {
            HasInternalOutcomeSet = true;
            InternalOutcome = outcome;

            if (!resultHolder.IsOfSameGenericType<O>())
            {
                resultHolder = new ResultCompletes<O>(InternalClientCompletes, InternalOutcome, HasInternalOutcomeSet);
            }

            return (ICompletes<O>)resultHolder;
        }

        public override ICompletes ClientCompletes()
        {
            return InternalClientCompletes;
        }

        public override void Reset(ICompletes clientCompletes)
        {
           InternalClientCompletes = clientCompletes;
           InternalOutcome = null;
           HasInternalOutcomeSet = false;
        }

        public override bool IsOfSameGenericType<TOtherType>()
            => typeof(T) == typeof(TOtherType);

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

        public TO AndThenTo<TO>(Func<T, TO> function)
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

        public T Await(TimeSpan timeout)
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
    }
}
