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
    }
}
