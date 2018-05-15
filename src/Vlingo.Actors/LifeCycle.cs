using System;

namespace Vlingo.Actors
{
    public sealed class LifeCycle
    {
        public Address Address { get; set; }
        public Environment Environment { get; set; }
        public bool IsStopped { get; set; }
        public Definition Definition { get; set; }
        public bool IsSuspended { get; }
        public bool IsResuming { get; internal set; }

        public LifeCycle(Environment environment)
        {
            Environment = environment;
        }

        public void Stop(Actor actor)
        {
            throw new System.NotImplementedException();
        }

        public void Secure()
        {
            throw new System.NotImplementedException();
        }

        public void AfterStop(Actor actor)
        {
            throw new System.NotImplementedException();
        }

        public void BeforeStart(Actor actor)
        {
            throw new System.NotImplementedException();
        }

        public ISupervisor LookUpProxy(Type type)
        {
            throw new System.NotImplementedException();
        }

        internal void SendStart(Actor actor)
        {
            throw new NotImplementedException();
        }

        internal void NextResuming()
        {
            throw new NotImplementedException();
        }
    }
}