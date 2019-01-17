// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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

        public virtual bool IsCompleted => throw new NotSupportedException();

        public virtual bool HasFailed => throw new NotSupportedException();

        public virtual bool HasOutcome => throw new NotSupportedException();

        public virtual T Outcome => throw new NotSupportedException();

        public virtual ICompletes<T> AndThen(long timeout, T failedOutcomeValue, Func<T, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThen(T failedOutcomeValue, Func<T, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThen(long timeout, Func<T, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThen(Func<T, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThenConsume(long timeout, T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThenConsume(T failedOutcomeValue, Action<T> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThenConsume(long timeout, Action<T> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> AndThenConsume(Action<T> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<F, O>(long timeout, F failedOutcomeValue, Func<T, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<F, O>(F failedOutcomeValue, Func<T, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<O>(long timeout, Func<T, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual O AndThenInto<O>(Func<T, O> function)
        {
            throw new NotSupportedException();
        }

        public virtual T Await()
        {
            throw new NotSupportedException();
        }

        public virtual T Await(long timeout)
        {
            throw new NotSupportedException();
        }

        public virtual void Failed()
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> Otherwise(Func<T, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> OtherwiseConsume(Action<T> consumer)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> RecoverFrom(Func<Exception, T> function)
        {
            throw new NotSupportedException();
        }

        public virtual ICompletes<T> Repeat()
        {
            throw new NotSupportedException();
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
    }
}
