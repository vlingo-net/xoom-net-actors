// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;

namespace Vlingo.Actors
{
    public abstract class Actor : IStartable, IStoppable, ITestStateView
    {
        internal ICompletes completes;
        internal LifeCycle LifeCycle { get; }

        public virtual Address Address => LifeCycle.Address;

        public virtual IDeadLetters DeadLetters => LifeCycle.Environment.Stage.World.DeadLetters;

        public virtual Scheduler Scheduler => LifeCycle.Environment.Stage.Scheduler;

        public virtual void Start()
        {
        }

        public virtual bool IsStopped => LifeCycle.IsStopped;

        public virtual void Stop()
        {
            if (!IsStopped)
            {
                if (LifeCycle.Address.Id != World.DeadLettersId)
                {
                    LifeCycle.Stop(this);
                }
            }
        }

        public virtual TestState ViewTestState() => new TestState();

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Address.Equals(((Actor) other).LifeCycle.Address);
        }

        public override int GetHashCode() => LifeCycle.GetHashCode();

        public override string ToString() => $"Actor[type={GetType().Name} address={Address}]";

        internal Actor Parent
        {
            get
            {
                if (LifeCycle.Environment.IsSecured)
                {
                    throw new InvalidOperationException("A secured actor cannot provide its parent.");
                }

                return LifeCycle.Environment.Parent;
            }
        }

        protected Actor()
        {
            var maybeEnvironment = ActorFactory.ThreadLocalEnvironment.Value;
            LifeCycle = new LifeCycle(maybeEnvironment ?? new TestEnvironment());
            ActorFactory.ThreadLocalEnvironment.Value = null;
        }

        protected T ChildActorFor<T>(Definition definition)
        {
            if (definition.Supervisor != null)
            {
                return LifeCycle.Environment.Stage.ActorFor<T>(definition, this, definition.Supervisor,
                    Logger);
            }
            else
            {
                if (this is ISupervisor)
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(
                        definition, 
                        this,
                        LifeCycle.LookUpProxy<ISupervisor>(),
                        Logger);
                }
                else
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(definition, this, null, Logger);
                }
            }
        }

        internal protected ICompletes<T> Completes<T>()
        {
            if(completes == null)
            {
                throw new InvalidOperationException("Completes is not available for this protocol behavior");
            }

            return (ICompletes<T>)completes;
        }

        protected Definition Definition => LifeCycle.Definition;

        internal protected ILogger Logger => LifeCycle.Environment.Logger;

        protected T ParentAs<T>()
        {
            if (LifeCycle.Environment.IsSecured)
            {
                throw new InvalidOperationException("A secured actor cannot provide its parent.");
            }

            var parent = LifeCycle.Environment.Parent;
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(parent, parent.LifeCycle.Environment.Mailbox);
        }

        protected void Secure()
        {
            LifeCycle.Secure();
        }

        internal protected T SelfAs<T>()
        {
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(this, LifeCycle.Environment.Mailbox);
        }

        protected IOutcomeInterest<TOutcome> SelfAsOutcomeInterest<TOutcome, TRef>(TRef reference)
        {
            var outcomeAware = LifeCycle.Environment.Stage.ActorProxyFor<IOutcomeAware<TOutcome, TRef>>(
                this,
                LifeCycle.Environment.Mailbox);

            return new OutcomeInterestActorProxy<TOutcome, TRef>(outcomeAware, reference);
        }

        internal protected Stage Stage
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

        internal protected Stage StageNamed(string name)
        {
            return LifeCycle.Environment.Stage.World.StageNamed(name);
        }

        //=======================================
        // stowing/dispersing
        //=======================================

        internal protected virtual bool IsDispersing =>
            LifeCycle.Environment.Stowage.IsDispersing;


        internal protected virtual void DisperseStowedMessages()
        {
            LifeCycle.Environment.Stowage.DispersingMode();
        }

        internal protected virtual bool IsStowing =>
            LifeCycle.Environment.Stowage.IsStowing;


        internal protected virtual void StowMessages()
        {
            LifeCycle.Environment.Stowage.StowingMode();
        }

        //=======================================
        // life cycle overrides
        //=======================================

        internal protected virtual void BeforeStart()
        {
            // override
        }

        internal protected virtual void AfterStop()
        {
            // override
        }

        internal protected virtual void BeforeRestart(Exception reason)
        {
            // override
            LifeCycle.AfterStop(this);
        }

        internal protected virtual void AfterRestart(Exception reason)
        {
            // override
            LifeCycle.BeforeStart(this);
        }

        internal protected virtual void BeforeResume(Exception reason)
        {
            // override
        }
    }
}