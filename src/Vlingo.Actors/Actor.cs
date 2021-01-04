// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors
{
    /// <summary>
    /// The abstract base class of all concrete <c>Actor</c> types. This base provides common
    /// facilities and life cycle processing for all <c>Actor</c> types.
    /// </summary>
    public abstract class Actor : IStartable, IStoppable, IRelocatable, ITestStateView
    {
        internal readonly ResultCompletes CompletesImpl;
        internal LifeCycle LifeCycle { get; }

        /// <summary>
        /// Answers the <c>address</c> of this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>Address</c> of this <c>Actor</c>.</value>
        public IAddress Address => LifeCycle.Address;

        /// <summary>
        /// Answers the <c>DeadLetters</c> for this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>DeadLetters</c> for this <c>Actor</c>.</value>
        public virtual IDeadLetters? DeadLetters => LifeCycle.Environment.Stage.World.DeadLetters;

        /// <summary>
        /// Answers the <c>Scheduler</c> for this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>Scheduler</c> for this <c>Actor</c>.</value>
        public virtual Scheduler Scheduler => LifeCycle.Environment.Stage.Scheduler;
        
        /// <inheritdoc cref="IRelocatable"/>
        public void StateSnapshot<S>(S stateSnapshot)
        {
            // no-op
        }

        /// <inheritdoc cref="IRelocatable"/>
        public S StateSnapshot<S>() => default!; // no-op

        /// <summary>
        /// The default implementation of <c>Start()</c>, which is a no-op. Override if needed.
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// Answers whether or not this <c>Actor</c> has been stopped or is in the process or stopping.
        /// </summary>
        /// <value><c>true</c> if this <c>Actor</c> is stopped. <c>false</c> otherwise.</value>
        public virtual bool IsStopped => LifeCycle.IsStopped;

        public virtual void Conclude()
            => SelfAs<IStoppable>().Stop();

        /// <summary>
        /// Initiates the process of stopping this <code>Actor</code> and all of its children.
        /// </summary>
        public virtual void Stop()
        {
            if (!IsStopped)
            {
                if (LifeCycle.Address.Id != World.DeadLettersId)
                {
                    // TODO: remove this actor as a child on parent
                    LifeCycle.SuspendForStop();
                    LifeCycle.Stop(this);
                }
            }
        }

        /// <summary>
        /// Received from the surrounding <see cref="TestActor{T}"/> to indicate
        /// that it is in use, enabling any special test initialization as
        /// eded. This is received (1) with a <code>TestContext</code> when the
        /// <code>TestActor</code> is first constructed, and (2) with <code>null</code>
        /// before each <code>TestMailbox</code> delivery.
        /// </summary>
        /// <param name="context">The TestContext.</param>
        public virtual void ViewTestStateInitialization(TestContext? context) { }

        /// <summary>
        /// Answers the <c>TestState</c> for this <c>Actor</c>. Override to provide a snapshot of the current <c>Actor</c> state.
        /// </summary>
        /// <returns>The <c>TestState</c> of this <c>Actor</c>.</returns>
        public virtual TestState ViewTestState() => new TestState();

        /// <summary>
        /// Answers whether or not this <c>Actor</c> is equal to <c>other</c>.
        /// </summary>
        /// <param name="other">The <c>object</c> to which this <c>Actor</c> is compared</param>
        /// <returns><c>true</c> if the two objects are of same type and has the same <c>address</c>. <c>false</c> otherwise.</returns>
        public override bool Equals(object? other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Address.Equals(((Actor)other).LifeCycle.Address);
        }

        /// <summary>
        /// Answers the <c>int</c> hash code of this <c>Actor</c>.
        /// </summary>
        /// <returns>The hash code of this <c>Actor</c>.</returns>
        public override int GetHashCode() => LifeCycle.GetHashCode();

        /// <summary>
        /// Answers the <c>string</c> representation of this <c>Actor</c>.
        /// </summary>
        /// <returns>The <c>string</c> representation of this <c>Actor</c></returns>
        public override string ToString() => $"Actor[type={GetType().Name} address={Address}]";

        /// <summary>
        /// Answers the parent <c>Actor</c> of this <c>Actor</c>. (INTERNAL ONLY)
        /// </summary>
        /// <value>Gets the parent <c>Actor</c>.</value>
        internal virtual Actor Parent
        {
            get
            {
                if (LifeCycle.Environment.IsSecured)
                {
                    throw new InvalidOperationException("A secured actor cannot provide its parent.");
                }

                return LifeCycle.Environment.Parent!;
            }
        }

        /// <summary>
        /// Initializes the newly created <c>Actor</c>.
        /// </summary>
        protected Actor()
        {
            var maybeEnvironment = ActorFactory.ThreadLocalEnvironment.Value;
            LifeCycle = new LifeCycle(maybeEnvironment ?? new TestEnvironment(), new Evictable(this));
            ActorFactory.ThreadLocalEnvironment.Value = null;
            CompletesImpl = new ResultCompletes<object>();
        }
        
        /// <summary>
        /// Answer my internal <see cref="ICompletes"/> from <c>Completes()</c> after preparing
        /// for the <paramref name="eventualOutcome"/> to be set in my <c>CompletesEventually()</c>.
        /// </summary>
        /// <param name="eventualOutcome">The <see cref="ICompletes"/> the provides an eventual outcome</param>
        /// <typeparam name="R">The return type of <see cref="ICompletes"/></typeparam>
        /// <returns><see cref="ICompletes"/> of type <typeparamref name="R"/></returns>
        protected internal ICompletes<R> AnswerFrom<R>(ICompletes<R> eventualOutcome)
        {
            eventualOutcome.AndThenConsume(value => CompletesEventually().With(value));
            return (ICompletes<R>)Completes();
        }

        /// <summary>
        /// Answers the <typeparamref name="T"/> protocol for the child <c>Actor</c> to be created by this parent <c>Actor</c>.
        /// </summary>
        /// <typeparam name="T">The protocol type</typeparam>
        /// <param name="definition">The <c>Definition</c> of the child <c>Actor</c> to be created by this parent <c>Actor</c></param>
        /// <returns>A child <c>Actor</c> of type <typeparamref name="T"/> created by this parent <c>Actor</c>.</returns>
        protected internal virtual T ChildActorFor<T>(Definition definition)
        {
            if (definition.Supervisor != null)
            {
                return LifeCycle.Environment.Stage.ActorFor<T>(
                    definition,
                    this,
                    definition.Supervisor,
                    Logger);
            }

            if (this is ISupervisor)
            {
                return LifeCycle.Environment.Stage.ActorFor<T>(
                    definition,
                    this,
                    LifeCycle.LookUpProxy<ISupervisor>(),
                    Logger);
            }

            return LifeCycle.Environment.Stage.ActorFor<T>(definition, this, null, Logger);
        }

        protected internal T ChildActorFor<T>(Expression<Func<T>> factory)
            => ChildActorFor<T>(Definition.Has(factory));

        /// <summary>
        /// Answers the protocol for the child <c>Actor</c> to be created by this parent <c>Actor</c>.
        /// </summary>
        /// <param name="protocol">The type of the child <c>Actor</c> to be created.</param>
        /// <param name="definition">The <c>Definition</c> of the child <c>Actor</c> to be created by this parent <c>Actor</c></param>
        /// <returns>A child <c>Actor</c> of type <paramref name="protocol"/> created by this parent <c>Actor</c>.</returns>
        protected internal virtual object? ChildActorFor(Type protocol, Definition definition)
        {
            var method = GetType().GetMethod(
                "ChildActorFor",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                null,
                new[] { typeof(Definition) },
                null);

            if (method == null)
            {
                throw new InvalidOperationException("Cannot find 'ChildActorFor' method on Actor");
            }
                
            return method.MakeGenericMethod(protocol).Invoke(definition, new object[] {definition});
        }

        /// <summary>
        /// Answers the <code>Protocols</code> for the child <code>Actor</code> to be created by this parent <code>Actor</code>.
        /// </summary>
        /// <param name="protocols">The protocols for the child <code>Actor</code>.</param>
        /// <param name="definition">The <code>Definition</code> of the child <code>Actor</code> to be created by this parent <code>Actor</code>.</param>
        /// <returns><code>Protocols</code></returns>
        protected internal Protocols ChildActorFor(Type[] protocols, Definition definition)
        {
            if (definition.Supervisor != null)
            {
                return LifeCycle.Environment.Stage.ActorFor(protocols, definition, this, definition.Supervisor, Logger);
            }

            if (this is ISupervisor)
            {
                return LifeCycle.Environment.Stage.ActorFor(protocols, definition, this, LifeCycle.LookUpProxy<ISupervisor>(), Logger);
            }

            return LifeCycle.Environment.Stage.ActorFor(protocols, definition, this, null, Logger);
        }

        /// <summary>
        /// Answers the <c>ICompletes</c> instance for this <c>Actor</c>, or <c>null</c> if the behavior of the currently
        /// delivered <c>IMessage</c> does not answer a <c>ICompletes</c>
        /// </summary>
        /// <returns><c>ICompletes</c> or <c>null</c>, depending on the current <c>IMessage</c> delivered.</returns>
        protected internal virtual ICompletes Completes()
        {
            if (CompletesImpl == null || CompletesImpl.InternalClientCompletes == null)
            {
                throw new InvalidOperationException("Completes is not available for this protocol behavior; return type must not be void.");
            }

            return CompletesImpl.ClientCompletes()!;
        }

        /// <summary>
        /// Answers a <c>ICompletesEventually</c> if the behavior of the currently
        /// delivered <c>IMessage</c> does answers a <c>ICompletes</c>. Otherwise the outcome
        /// is undefined.
        /// </summary>
        /// <returns>A <c>ICompletesEventually</c> instance.</returns>
        protected internal virtual ICompletesEventually CompletesEventually()
            => LifeCycle.Environment.CompletesEventually(CompletesImpl);

        /// <summary>
        /// Answers the <c>Definition</c> of this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>Definition</c> of this <c>Actor</c></value>
        protected internal virtual Definition Definition => LifeCycle.Definition;

        /// <summary>
        /// Answers the <c>Logger</c> of this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>ILogger</c> instance of this <c>Actor</c></value>
        protected internal ILogger Logger => LifeCycle.Environment.Logger;

        /// <summary>
        /// Answers the parent of this <c>Actor</c> as the <typeparamref name="T"/> protocol.
        /// </summary>
        /// <typeparam name="T">The protocol type for the parent <c>Actor</c>.</typeparam>
        /// <returns>The parent <c>Actor</c> as <typeparamref name="T"/>.</returns>
        protected internal virtual T ParentAs<T>()
        {
            if (LifeCycle.Environment.IsSecured)
            {
                throw new InvalidOperationException("A secured actor cannot provide its parent.");
            }

            var parent = LifeCycle.Environment.Parent!;
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(parent, parent.LifeCycle.Environment.Mailbox);
        }

        /// <summary>
        /// Secures this <c>Actor</c>.
        /// </summary>
        protected virtual void Secure() => LifeCycle.Secure();

        /// <summary>
        /// Answers this <c>Actor</c> as a <typeparamref name="T"/> protocol. This <c>Actor</c> must implement the <typeparamref name="T"/> protocol.
        /// </summary>
        /// <typeparam name="T">The protocol type</typeparam>
        /// <returns>This <c>Actor</c> as <typeparamref name="T"/></returns>
        protected internal T SelfAs<T>() => LifeCycle.Environment.Stage.ActorProxyFor<T>(this, LifeCycle.Environment.Mailbox);

        /// <summary>
        /// Answers the <c>Stage</c> of this <c>Actor</c>.
        /// </summary>
        /// <value>Gets the <c>Stage</c> of this <c>Actor</c></value>
        protected internal Stage Stage
        {
            get
            {
                if (LifeCycle.Environment.IsSecured)
                {
                    throw new InvalidOperationException("A secured actor cannot provide its stage.");
                }

                return LifeCycle.Environment.Stage;
            }
        }

        /// <summary>
        /// Answers the <c>Stage</c> of the given name.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the <c>Stage</c> to find.</param>
        /// <returns>The <c>Stage</c> with the given <paramref name="name"/></returns>
        protected internal virtual Stage StageNamed(string name) => LifeCycle.Environment.Stage.World.StageNamed(name);

        //=======================================
        // stowing/dispersing
        //=======================================

        /// <summary>
        /// Starts the process of dispersing any messages stowed for this <c>Actor</c>.
        /// </summary>
        protected internal virtual void DisperseStowedMessages() => LifeCycle.Environment.Mailbox.Resume(Mailbox.Paused);

        /// <summary>
        /// Starts the process of stowing messages for this <c>Actor</c>, and registers <paramref name="stowageOverrides"/> as
        /// the protocol that will trigger dispersal.
        /// </summary>
        /// <param name="stowageOverrides">The protocol <c>Type</c>(s) that will trigger dispersal</param>
        protected internal virtual void StowMessages(params Type[] stowageOverrides) =>
            LifeCycle.Environment.Mailbox.SuspendExceptFor(Mailbox.Paused, stowageOverrides);

        //=======================================
        // life cycle overrides
        //=======================================

        /// <summary>
        /// The message delivered before the <c>Actor</c> has fully started. Override to implement.
        /// </summary>
        protected internal virtual void BeforeStart()
        {
            // override
        }

        /// <summary>
        /// The message delivered after the <c>Actor</c> has fully stopped. Override to implement.
        /// </summary>
        protected internal virtual void AfterStop()
        {
            // override
        }

        /// <summary>
        /// The message delivered before the <c>Actor</c> has been restarted by its supervisor due to an exception.
        /// Override to implement.
        /// </summary>
        /// <param name="reason">The <c>Exception</c> cause of the supervision restart.</param>
        protected internal virtual void BeforeRestart(Exception reason)
        {
            // override for specific recovery
            Logger.Error($"Default before restart recovery after: {reason.Message}", reason);
            LifeCycle.AfterStop(this);
        }

        /// <summary>
        /// The message delivered after the <c>Actor</c> has been restarted by its supervisor due to an exception.
        /// Override to implement.
        /// </summary>
        /// <param name="reason">The <c>Exception</c> cause of the supervision restart.</param>
        protected internal virtual void AfterRestart(Exception reason)
        {
            // override for specific recovery
            Logger.Error($"Default after restart recovery after: {reason.Message}", reason);
            LifeCycle.BeforeStart(this);
        }

        /// <summary>
        /// The message delivered before the <c>Actor</c> has been resumed by its supervisor due to an exception.
        /// Override to implement.
        /// </summary>
        /// <param name="reason">The <c>Exception</c> cause of the supervision resume.</param>
        protected internal virtual void BeforeResume(Exception reason)
        {
            // override for specific recovery
            Logger.Error($"Default before resume recovery after: {reason.Message}", reason);
        }
    }
}