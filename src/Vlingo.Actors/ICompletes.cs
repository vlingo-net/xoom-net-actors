using System;

namespace Vlingo.Actors
{
    public interface ICompletes<T>
    {
        ICompletes<T> After(Func<T> supplier);
        ICompletes<T> After(Func<T> supplier, long timeout);
        ICompletes<T> After(Func<T> supplier, long timeout, T timedOutValue);
        ICompletes<T> AndThen(Action<T> consumer);
        ICompletes<T> After(Action<T> consumer);
        ICompletes<T> After(Action<T> consumer, long timeout);
        ICompletes<T> After(Action<T> consumer, long timeout, T timedOutValue);
        bool HasOutcome { get; }
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
