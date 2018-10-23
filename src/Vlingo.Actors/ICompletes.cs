// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public interface ICompletes
    {
        bool HasOutcome { get; }
        object Outcome { get; }
        ICompletes With(object outcome);
    }

    public interface ICompletes<T> : ICompletes
    {
        ICompletes<T> After(Func<T> supplier);
        ICompletes<T> After(Func<T> supplier, long timeout);
        ICompletes<T> After(Func<T> supplier, long timeout, T timedOutValue);
        ICompletes<T> After(Action<T> consumer);
        ICompletes<T> After(Action<T> consumer, long timeout);
        ICompletes<T> After(Action<T> consumer, long timeout, T timedOutValue);
        ICompletes<T> AndThen(Action<T> consumer);
        ICompletes<T> AtLast(Action<T> consumer);
        ICompletes<T> AtLast(Func<T> supplier);
        T Outcome { get; }
        ICompletes<TOutcome> With<TOutcome>(TOutcome outcome);
    }

    public static class Completes
    {
        public static ICompletes<T> Using<T>(Scheduler scheduler) => new BasicCompletes<T>(scheduler);
        public static ICompletes<T> WithSuccess<T>(T outcome) => new BasicCompletes<T>(outcome);
        public static ICompletes<T> WithFailure<T>(T outcome) => new BasicCompletes<T>(outcome);
        public static ICompletes<T> WithFailure<T>() => new BasicCompletes<T>(default(T));
    }
}
