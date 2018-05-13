using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors
{
    public abstract class Actor : IStartable, IStoppable
    {
        public LifeCycle LifeCycle { get; }

        public Address Address => LifeCycle.Address;

        public DeadLetters DeadLetters => LifeCycle.Environment.Stage.World.DeadLetters;

        public virtual void Start()
        {
        }

        public bool IsStopped => LifeCycle.IsStopped;

        public void Stop()
        {
            if (!IsStopped)
            {
                if (LifeCycle.Address.Id != World.DeadlettersId)
                {
                    LifeCycle.Stop(this);
                }
            }
        }


        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Address.Equals(((Actor) other).LifeCycle.Address);
        }

        public override int GetHashCode()
        {
            return LifeCycle.GetHashCode();
        }

        public override string ToString() => $"Actor[type={GetType().Name} address={Address}]";

        protected Actor()
        {
            // Not sure what is that :)
            //final Environment maybeEnvironment = ActorFactory.threadLocalEnvironment.get();
            //this.lifeCycle = new LifeCycle(maybeEnvironment != null ? maybeEnvironment : new TestEnvironment());
            //ActorFactory.threadLocalEnvironment.set(null);
            //this.lifeCycle.sendStart(this);
        }

        protected T ChildActorFor<T>(Definition definition, Type protocol)
        {
            if (definition.Supervisor != null)
            {
                return LifeCycle.Environment.Stage.ActorFor<T>(definition, protocol, this, definition.Supervisor,
                    Logger);
            }
            else
            {
                //obj.GetType().IsAssignableFrom(otherObj.GetType());
                if (this.GetType().IsAssignableFrom(typeof(Supervisor)))
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(definition, protocol, this,
                        LifeCycle.LookUpProxy(typeof(Supervisor)), Logger);
                }
                else
                {
                    return LifeCycle.Environment.Stage.ActorFor<T>(definition, protocol, this, null, Logger);
                }
            }
        }

        protected Definition Definition => LifeCycle.Definition;

        protected Logger Logger => LifeCycle.Environment.Logger;

        protected Actor Parent
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

        // Suggest make this SetAsSecure
        protected void Secure()
        {
            LifeCycle.Secure();
        }

        protected T SelfAs<T>(T protocol)
        {
            return LifeCycle.Environment.Stage.ActorProxyFor(protocol, this, LifeCycle.Environment.Mailbox);
        }

        // Need to discuss this
//        protected IOutcomeInterest SelfAsOutcomeInterest(object reference) {
//            var outcomeAware = LifeCycle.Environment.Stage.ActorProxyFor<IOutcomeAware<>>
//                (typeof(IOutcomeAware<>), this, LifeCycle.Environment.Mailbox);
//            
//            return new OutcomeInterestActorProxy(outcomeAware, reference);
//        }

        protected Stage Stage
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

        protected Stage StageNamed(string name)
        {
            return LifeCycle.Environment.Stage.World.StageNamed(name);
        }


        protected bool IsDispersing =>
            LifeCycle.Environment.Stowage.IsDispersing;


        protected void DisperseStowedMessages()
        {
            LifeCycle.Environment.Stowage.DispersingMode();
        }

        protected bool IsStowing =>
            LifeCycle.Environment.Stowage.IsStowing;


        protected void StowMessages()
        {
            LifeCycle.Environment.Stowage.StowingMode();
        }

        //=======================================
        // life cycle overrides
        //=======================================

        protected void BeforeStart()
        {
            // override
        }

        protected void AfterStop()
        {
            // override
        }

        protected void BeforeRestart(Exception reason)
        {
            // override
            LifeCycle.AfterStop(this);
        }

        protected void AfterRestart(Exception reason)
        {
            // override
            LifeCycle.BeforeStart(this);
        }

        protected void BeforeResume(Exception reason)
        {
            // override
        }
    }
}