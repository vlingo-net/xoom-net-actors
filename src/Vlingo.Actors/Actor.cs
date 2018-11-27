// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public abstract class Actor : IStartable, IStoppable, ITestStateView
    {
        internal ResultCompletes completes;
        internal LifeCycle LifeCycle { get; }

        public virtual IAddress Address => LifeCycle.Address;

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
                    // TODO: remove this actor as a child on parent
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

        internal virtual Actor Parent
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
            completes = new ResultCompletes();
        }

        protected internal virtual T ChildActorFor<T>(Definition definition)
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

        protected internal virtual ICompletes<T> Completes<T>()
        {
            if(completes == null)
            {
                throw new InvalidOperationException("Completes is not available for this protocol behavior");
            }

            return (ICompletes<T>)completes;
        }

        protected internal virtual ICompletesEventually CompletesEventually()
            => LifeCycle.Environment.Stage.World.CompletesFor(completes.ClientCompletes());

        protected internal virtual Definition Definition => LifeCycle.Definition;

        protected internal virtual ILogger Logger => LifeCycle.Environment.Logger;

        protected internal virtual T ParentAs<T>()
        {
            if (LifeCycle.Environment.IsSecured)
            {
                throw new InvalidOperationException("A secured actor cannot provide its parent.");
            }

            var parent = LifeCycle.Environment.Parent;
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(parent, parent.LifeCycle.Environment.Mailbox);
        }

        protected virtual void Secure()
        {
            LifeCycle.Secure();
        }

        protected internal virtual T SelfAs<T>()
        {
            return LifeCycle.Environment.Stage.ActorProxyFor<T>(this, LifeCycle.Environment.Mailbox);
        }

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

        protected internal virtual Stage StageNamed(string name)
        {
            return LifeCycle.Environment.Stage.World.StageNamed(name);
        }

        //=======================================
        // stowing/dispersing
        //=======================================

        protected internal virtual bool IsDispersing =>
            LifeCycle.IsDispersing;


        protected internal virtual void DisperseStowedMessages()
        {
            LifeCycle.DisperseStowedMessages();
        }

        protected internal virtual bool IsStowing =>
            LifeCycle.IsStowing;


        protected internal virtual void StowMessages(params Type[] stowageOverrides)
        {
            LifeCycle.StowMessages();
            LifeCycle.Environment.StowageOverrides(stowageOverrides);
        }

        //=======================================
        // life cycle overrides
        //=======================================

        protected internal virtual void BeforeStart()
        {
            // override
        }

        protected internal virtual void AfterStop()
        {
            // override
        }

        protected internal virtual void BeforeRestart(Exception reason)
        {
            // override
            LifeCycle.AfterStop(this);
        }

        protected internal virtual void AfterRestart(Exception reason)
        {
            // override
            LifeCycle.BeforeStart(this);
        }

        protected internal virtual void BeforeResume(Exception reason)
        {
            // override
        }
    }
}