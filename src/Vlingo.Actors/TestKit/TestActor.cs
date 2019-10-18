// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.TestKit
{
    /// <summary>
    /// Actor that has an immediately delivered mailbox for use in testing.
    /// </summary>
    /// <typeparam name="T">The type of actor protocol.</typeparam>
    public class TestActor<T> : ITestStateView
    {
        /// <summary>
        /// Construct my default state.
        /// </summary>
        /// <param name="actor">The Actor inside being tested.</param>
        /// <param name="protocol">The T protocol being tested.</param>
        /// <param name="address">The address of the actor.</param>
        public TestActor(Actor actor, T protocol, IAddress address)
        {
            ActorInside = actor;
            Actor = protocol;
            Address = address;
            Context = new TestContext();
        }

        /// <summary>
        /// Answer my <code>actor</code> inside as the <typeparamref name="T"/> protocol.
        /// </summary>
        public virtual T Actor { get; }

        /// <summary>
        /// Answer my actor inside as protocol <typeparamref name="TActor"/>.
        /// </summary>
        /// <typeparam name="TActor">The protocol for my actor inside.</typeparam>
        /// <returns></returns>
        public virtual TActor ActorAs<TActor>()
            => (TActor)(object)Actor!;

        /// <summary>
        /// Answer my address, which is the address of my actor inside.
        /// </summary>
        public virtual IAddress Address { get; }

        /// <summary>
        /// Answer my actor inside.
        /// </summary>
        public virtual Actor ActorInside { get; }

        /// <summary>
        /// Answer my context after resetting the expected completions/happenings.
        /// </summary>
        /// <param name="times">The int number of expected completions/happenings.</param>
        /// <returns></returns>
        public virtual TestContext AndNowCompleteWithHappenings(int times)
        {
            Context = Context.ResetAfterCompletingTo(times);
            return Context;
        }

        /// <summary>
        /// Answer the <typeparamref name="V"/> typed value of my context when it is available.
        /// Block unless the value is immediately available.
        /// </summary>
        /// <typeparam name="V">The value type.</typeparam>
        /// <returns></returns>
        public virtual V MustComplete<V>()
            => Context.MustComplete<V>();

        /// <summary>
        /// Answer the <code>TestState</code> of my actor inside.
        /// </summary>
        /// <returns></returns>
        public virtual TestState ViewTestState() => ActorInside.ViewTestState();

        /// <summary>
        /// Answer my context.
        /// </summary>
        public virtual TestContext Context { get; private set; }
    }
}
