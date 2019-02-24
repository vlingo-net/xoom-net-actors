// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors.TestKit
{
    /// <summary>
    /// Facilitate thread-safe accessing of shared data, both for writing and reading. The
    /// Factory Method <code>AfterCompleting()</code> is used to determine how many times the
    /// <code>WriteUsing()</code> behavior is employed before the <code>readUsing()</code> can complete.
    /// </summary>
    public class AccessSafely
    {
        private readonly object @lock;
        private readonly IDictionary<string, object> consumers;
        private readonly IDictionary<string, object> functions;
        private readonly IDictionary<string, object> suppliers;
        private readonly TestUntil until;

        private AccessSafely(int happenings)
        {
            until = TestUntil.Happenings(happenings);
            consumers = new Dictionary<string, object>();
            functions = new Dictionary<string, object>();
            suppliers = new Dictionary<string, object>();
            @lock = new object();
        }

        private AccessSafely() : this(0)
        {
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
        /// Answer me with <paramref name="function"/> registered for reading.
        /// </summary>
        /// <typeparam name="T">The type of the function parameter.</typeparam>
        /// <typeparam name="R">The type of function return value.</typeparam>
        /// <param name="name">The name of the function to register.</param>
        /// <param name="function">The <code>System.Func&lt;T, R&gt;</code> to register as a function.</param>
        /// <returns></returns>
        public virtual AccessSafely ReadingWith<T,R>(string name, Func<T,R> function)
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
        /// Answer the value associated with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with name.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <returns></returns>
        public virtual T ReadFrom<T>(string name)
        {
            if (!suppliers.ContainsKey(name))
            {
                throw new ArgumentException($"Unknow supplier: {name}", nameof(name));
            }

            until.Completes();

            lock (@lock)
            {
                return (suppliers[name] as Func<T>).Invoke();
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
        public virtual R ReadFrom<T,R>(string name, T parameter)
        {
            if (!functions.ContainsKey(name))
            {
                throw new ArgumentException($"Unknow function: {name}", nameof(name));
            }

            until.Completes();

            lock (@lock)
            {
                return (functions[name] as Func<T, R>).Invoke(parameter);
            }
        }

        /// <summary>
        /// Answer the value associated with <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with name.</typeparam>
        /// <param name="name">The name of the value to answer.</param>
        /// <returns></returns>
        public virtual T ReadFromNow<T>(string name)
        {
            if (!suppliers.ContainsKey(name))
            {
                throw new ArgumentException($"Unknow supplier: {name}", nameof(name));
            }

            lock (@lock)
            {
                return (suppliers[name] as Func<T>).Invoke();
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
                throw new ArgumentException($"Unknown consumer: {name}", nameof(name));
            }

            lock (@lock)
            {
                (consumers[name] as Action<T>).Invoke(value);
                until.Happened();
            }
        }
    }
}
