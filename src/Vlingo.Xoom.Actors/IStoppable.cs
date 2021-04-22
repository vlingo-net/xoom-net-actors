// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// Arranges for stopping the receiver.
    /// <para>
    /// Note that the contract of stop() is to suspend the receivers mailbox
    /// deliveries, which will prevent further processing. To arrange for
    /// eventually stopping the receiver send Conclude(), which will then
    /// cause Stop(). In essence the Conclude() marks the mailbox for ending
    /// operations, but allows messages already queued to first be delivered.
    /// </para>
    /// </summary>
    public interface IStoppable
    {
        /// <summary>
        /// Concludes the receiver, eventually causing
        /// it to receive a Stop() message.
        /// </summary>
        void Conclude();

        /// <summary>
        /// Answer whether or not the receiver is stopped.
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        /// Causes the receiver to stop reacting to messages and to eventually
        /// be garbage collected.
        /// <para>
        /// Note that the contract of Stop() is to suspend the receivers mailbox
        /// deliveries, which will prevent further processing. To arrange for
        /// eventually stopping the receiver send Conclude(), which will then
        /// cause Stop(). In essence the Conclude() marks the mailbox for ending
        /// operations, but allows messages already queued to first be delivered.
        /// </para>
        /// </summary>
        void Stop();
    }
}