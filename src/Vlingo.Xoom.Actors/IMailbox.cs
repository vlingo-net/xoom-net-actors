// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// Standard actor mailbox protocol.
    /// </summary>
    public interface IMailbox : IRunnable
    {
        /// <summary>
        /// Close me.
        /// </summary>
        void Close();

        /// <summary>
        /// Answers whether or not I am closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Answers whether or not I am delivering a message.
        /// </summary>
        bool IsDelivering { get; }
        
        /// <summary>
        /// Gets the total capacity for concurrent operations.
        /// </summary>
        int ConcurrencyCapacity { get; }

        /// <summary>
        /// Recovers the previous operational mode, either active or suspended,
        /// and if active resumes delivery. If the restored operational mode
        /// is still suspended, then at least one more <code>Resume()</code> is required.
        /// If the <paramref name="name"/> is not the current suspended mode, its corresponding
        /// overrides may be marked for removal after any later overrides are removed.
        /// </summary>
        /// <param name="name">The name of the overrides for which to resume.</param>
        void Resume(string name);

        /// <summary>
        /// Arrange for <code>IMessage</code> to be sent, which will generally
        /// be delivered by another thread. Exceptions to this rule are
        /// for possible, such as for <code>TestMailbox</code>.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(IMessage message);

        /// <summary>
        /// <para>
        /// Suspend message deliver but allow any of the given <paramref name="overrides"/>
        /// to pass through, essentially giving these priority. Note that the
        /// receiving Actor must use <code>Resume()</code> to cause normally delivery
        /// when it is ready.
        /// </para>
        /// <para>
        /// NOTE: If <code>SuspendExceptFor(overrides)</code> is used multiple times before
        /// the implementing <code>IMailbox</code> is resumed, the outcome is implementation
        /// dependent. The implementor may accumulate or replace its internal
        /// overrides with the <paramref name="overrides"/> parameter. The <paramref name="name"/> may be helpful
        /// in maintaining accumulated <paramref name="overrides"/>.
        /// </para>
        /// </summary>
        /// <param name="name">The name of the specific override.</param>
        /// <param name="overrides">The list of types that are allowed to be delivered although others are suspended.</param>
        void SuspendExceptFor(string name, params Type[] overrides);

        bool IsSuspendedFor(string name);
        
        /// <summary>
        /// Answer whether or not I am currently suspended.
        /// </summary>
        bool IsSuspended { get; }

        /// <summary>
        /// Answer the next <code>IMessage</code> that can be received.
        /// </summary>
        /// <returns></returns>
        IMessage? Receive();

        /// <summary>
        /// Answer the count of messages that have not yet been delivered.
        /// </summary>
        int PendingMessages { get; }

        /// <summary>
        /// Answer whether or not I am a <code>IMailbox</code> with pre-allocated and reusable message elements.
        /// </summary>
        bool IsPreallocated { get; }
        
        /// <summary>
        /// Arrange for <code>IMessage</code> to be sent by setting the pre-allocated
        /// and reusable element with the parameters. This manner of sending
        /// is meant to be used only when <see cref="IMailbox.IsPreallocated"/> answers <code>true</code>.
        /// </summary>
        /// <typeparam name="T">Type of actor protocol.</typeparam>
        /// <param name="actor">The actor being sent the message.</param>
        /// <param name="consumer">The consumer to carry out the action.</param>
        /// <param name="completes">The completes through which return values are communicated; null if void return.</param>
        /// <param name="representation">The string representation of this message invocation.</param>
        void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation);
        void Send(Actor actor, Type protocol, LambdaExpression consumer, ICompletes? completes, string representation);
        
        /// <summary>
        /// Gets the mailbox task scheduler for executing <see cref="Task"/> based asynchronous operations.
        /// </summary>
        TaskScheduler TaskScheduler { get; }
    }

    public static class Mailbox
    {
        public const string Exceptional = "#exceptional";
        public const string Paused = "#paused";
        public const string Stopping = "#stopping";
        public const string Task = "#task";
    }
}