// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using Vlingo.Common;

namespace Vlingo.Actors.TestKit
{
    /// <summary>
    /// Facilitate thread-safe accessing of shared data, both for writing and reading. The
    /// Factory Method <code>AfterCompleting()</code> is used to determine how many times the
    /// <code>WriteUsing()</code> behavior is employed before the <code>readUsing()</code> can complete.
    /// </summary>
    public class AccessSafely
    {
        private readonly AtomicInteger totalWrites;
        private readonly object @lock;
        private readonly IDictionary<string, object> biConsumers;
        private readonly IDictionary<string, object> consumers;
        private readonly IDictionary<string, object> functions;
        private readonly IDictionary<string, object> suppliers;
        private readonly TestUntil until;

        private AccessSafely(AccessSafely existing, int happenings)
        {
            totalWrites = existing.totalWrites;
            until = TestUntil.Happenings(happenings);
            biConsumers = existing.biConsumers;
            consumers = existing.consumers;
            functions = existing.functions;
            suppliers = existing.suppliers;
            @lock = new object();
        }

        private AccessSafely(int happenings)
        {
            totalWrites = new AtomicInteger(0);
            until = TestUntil.Happenings(happenings);
            biConsumers = new Dictionary<string, object>();
            consumers = new Dictionary<string, object>();
            functions = new Dictionary<string, object>();
            suppliers = new Dictionary<string, object>();
            @lock = new object();
        }

        private AccessSafely() : this(0)
        {
        }

        private Func<T, R> GetRequiredFunction<T, R>(string name)
        {
            if (functions.TryGetValue(name, out var obj))
            {
                if (obj != null)
                {
                    return (Func<T, R>)obj;
                }
            }

            throw new ArgumentException($"Unknown function: {name}");
        }

        public Func<T> GetRequiredSupplier<T>(string name)
        {
            if (suppliers.TryGetValue(name, out var obj))
            {
                if (obj != null)
                {
                    return (Func<T>)obj;
                }
            }

            throw new ArgumentException($"Unknown supplier: {name}");
        }

        /// <summary>
        /// Answer a new <code>AccessSafely</code> with a completion count of <paramref name="happenings"/>. This construct 
        /// provides a reliable barrier/fence around data access between two or more threads, given that the 
        /// number of <code>happenings</code> is accurately predicted.
        /// </summary>
        /// <param name="happenings">the int number of times that writeUsing() is employed prior to readFrom() answering.</param>
        /// <returns>AccessSafely</returns>
        public static AccessSafely AfterCompleting(int happenings) => new AccessSafely(happenings);

        /// <summary>
        /// Answer a new <code>AccessSafely</code> with immediate <code>readFrom()</code> results. Note 
        /// that this is not recommended due to highly probably inconsistencies in the data
        /// seen by the reader as opposed to that written by the writer.
        /// </summary>
        /// <returns>AccessSafely</returns>
        public static AccessSafely Immediately() => new AccessSafely();

        /// <summary>
        /// Answer a new AccessSafely with my existing reads and writes functionality.
        /// </summary>
        /// <param name="happenings">The number of times that WriteUsing() is employed prior to ReadFrom() answering</param>
        /// <returns></returns>
        public virtual AccessSafely ResetAfterCompletingTo(int happenings)
            => new AccessSafely(this, happenings);

        /// <summary>
        /// Answer me with <paramref name="function"/> registered for reading.
        /// </summary>
        /// <typeparam name="T">The type of the function parameter.</typeparam>
        /// <typeparam name="R">The type of function return value.</typeparam>
        /// <param name="name">The name of the function to register.</param>
        /// <param name="function">The <code>System.Func&lt;T, R&gt;</code> to register as a function.</param>
        /// <returns></returns>
        public virtual AccessSafely ReadingWith<T, R>(string name, Func<T, R> function)
        {
            functions[name] = function;
            return this;
        }

        /// <summary>
        /// Answer me with <paramref name="supplier"/> registered for reading.
        /// </summary>
        /// <typeparam name="T">The type of the supplier (return value) to register.</typeparam>
        /// <param name="name">The name of the supplier to register.</param>
        /// <param name="supplier">The <code>System.Func&lt;T&gt;</code> to register as supplier.</param>
        /// <returns></returns>
        public virtual AccessSafely ReadingWith<T>(string name, Func<T> supplier)
        {
            suppliers[name] = supplier;
            return this;
        }

        /// <summary>
        /// Answer me with <paramref name="supplier"/> registered for reading.
        /// </summary>
        /// <typeparam name="T">The type of the supplier to register.</typeparam>
        /// <param name="name">The name of the supplier to register.</param>
        /// <param name="supplier">The <code>System.Action&lt;T&gt;</code> to register as supplier.</param>
        /// <returns></returns>
        public virtual AccessSafely ReadingWith<T>(string name, Action<T> supplier)
        {
            suppliers[name] = supplier;
            return this;
        }

        /// <summary>
        /// Answer me with <paramref name="consumer"/> registered for writing.
        /// </summary>
        /// <typeparam name="T">The type of the consumer parameter.</typeparam>
        /// <param name="name">The name of the cosumer to register.</param>
        /// <param name="consumer">The <code>System.Action&lt;T&gt;</code> to register as a cosumer.</param>
        /// <returns></returns>
        public virtual AccessSafely WritingWith<T>(string name, Action<T> consumer)
        {
            consumers[name] = consumer;
            return this;
        }

        /// <summary>
        /// Answer me with <paramref name="consumer"/> registered for writing.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the consumer.</typeparam>
        /// <typeparam name="T2">The type of the seconds parameter of the consumer.</typeparam>
        /// <param name="name">The name of the consumer to register.</param>
        /// <param name="consumer">The consumer of type <code>Action&lt;T1, T2&gt;</code> to register.</param>
        /// <returns></returns>
        public virtual AccessSafely WritingWith<T1, T2>(string name, Action<T1, T2> consumer)
        {
            biConsumers[name] = consumer;
            return this;
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with name.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <returns></returns>
        public virtual T ReadFrom<T>(string name)
        {
            var supplier = GetRequiredSupplier<T>(name);

            until.Completes();

            lock (@lock)
            {
                return supplier.Invoke();
            }
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to the function.</typeparam>
        /// <typeparam name="R">The type of the return value associated with the name.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="parameter">The <typeparamref name="T"/> typed function parameter.</param>
        /// <returns></returns>
        public virtual R ReadFrom<T, R>(string name, T parameter)
        {
            var function = GetRequiredFunction<T, R>(name);

            until.Completes();

            lock (@lock)
            {
                return function.Invoke(parameter);
            }
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/> but not until
        /// it reaches the <paramref name="expected"/> value.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the <paramref name="name"/>.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="expected">The expected value of type <typeparamref name="T"/>.</param>
        /// <returns></returns>
        public virtual T ReadFromExpecting<T>(string name, T expected)
            => ReadFromExpecting<T>(name, expected, long.MaxValue);

        /// <summary>
        /// Answer the value associated with <paramref name="name"/> but not until
        /// it reaches the <paramref name="expected"/> value or total number
        /// of <paramref name="retries"/> is reached.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the <paramref name="name"/>.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="expected">The expected value of type <typeparamref name="T"/>.</param>
        /// <param name="retries">The number of retries.</param>
        /// <returns></returns>
        public virtual T ReadFromExpecting<T>(string name, T expected, long retries)
        {
            var supplier = GetRequiredSupplier<T>(name);
            T value = default;
            using (var waiter = new AutoResetEvent(false))
            {
                for (long count = 0; count < retries; ++count)
                {
                    lock (@lock)
                    {
                        value = supplier.Invoke();
                        if (Equals(expected, value))
                        {
                            return value;
                        }
                    }

                    try
                    {
                        waiter.WaitOne(TimeSpan.FromMilliseconds(1));
                    }
                    catch { }
                }
            }

            throw new InvalidOperationException($"Did not reach expected value: {expected}. Reached {value}");
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with name.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <returns></returns>
        public virtual T ReadFromNow<T>(string name)
        {
            var supplier = GetRequiredSupplier<T>(name);

            lock (@lock)
            {
                return supplier.Invoke();
            }
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/> immediately.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to the <code>Func&lt;T, R&gt;</code>.</typeparam>
        /// <typeparam name="R">The type of the return value associated with <paramref name="name"/>.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="parameter">The T typed function parameter.</param>
        /// <returns></returns>
        public virtual R ReadFromNow<T, R>(string name, T parameter)
        {
            var function = GetRequiredFunction<T, R>(name);

            lock (@lock)
            {
                return function.Invoke(parameter);
            }
        }

        /// <summary>
        /// Set the value associated with <paramref name="name"/> to the parameter <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with name that is to be written.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="value">The <typeparamref name="T"/> typed value to write.</param>
        public virtual void WriteUsing<T>(string name, T value)
        {
            if (!consumers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown function: {name}");
            }

            lock (@lock)
            {
                totalWrites.IncrementAndGet();
                (consumers[name] as Action<T>)!.Invoke(value);
                until.Happened();
            }
        }

        /// <summary>
        /// Set the values associated with <paramref name="name"/> using the parameters <paramref name="value1"/> and <paramref name="value2"/>.
        /// </summary>
        /// <typeparam name="T1">The type of <paramref name="value1"/> to write.</typeparam>
        /// <typeparam name="T2">The type of the <paramref name="value2"/> to write.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <param name="value1">The <typeparamref name="T1"/> typed value to write.</param>
        /// <param name="value2">The <typeparamref name="T2"/> typed value to write.</param>
        public virtual void WriteUsing<T1, T2>(string name, T1 value1, T2 value2)
        {
            if (!biConsumers.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown function: {name}");
            }

            lock (@lock)
            {
                totalWrites.IncrementAndGet();
                (biConsumers[name] as Action<T1, T2>)!.Invoke(value1, value2);
                until.Happened();
            }
        }

        /// <summary>
        /// Answer the total of writes completed.
        /// </summary>
        public virtual int TotalWrites
        {
            get
            {
                lock (@lock)
                {
                    return totalWrites.Get();
                }
            }
        }

        /// <summary>
        /// Answer the total of writes completed after ensuring that it surpasses <paramref name="lesser"/>,
        /// or if <paramref name="retries"/> is reached first throw <code>InvalidOperationException</code>.
        /// </summary>
        /// <param name="lesser">The int value that must be surpassed.</param>
        /// <param name="retries">The long number of retries before failing.</param>
        /// <returns></returns>
        public virtual int TotalWritesGreaterThan(int lesser, long retries)
        {
            using (var waiter = new AutoResetEvent(false))
            {
                for (long count = 0; count < retries; ++count)
                {
                    lock (@lock)
                    {
                        var total = totalWrites.Get();
                        if (total > lesser)
                        {
                            return total;
                        }
                    }

                    try
                    {
                        waiter.WaitOne(TimeSpan.FromMilliseconds(1));
                    }
                    catch { }
                }
            }

            throw new InvalidOperationException($"Did not reach expected value: {lesser + 1}");
        }
    }
}
