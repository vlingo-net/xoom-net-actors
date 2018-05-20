using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors
{
    public class StageSupervisedActor<T> : ISupervised
    {
        public StageSupervisedActor(Actor actor, Exception ex)
        {

        }

        public Address Address => throw new NotImplementedException();

        public ISupervisor Supervisor => throw new NotImplementedException();

        public Exception Error => throw new NotImplementedException();

        public void Escalate()
        {
            throw new NotImplementedException();
        }

        public void RestartWithin(long period, int intensity, SupervisionStrategyConstants.Scope scope)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Stop(SupervisionStrategyConstants.Scope scope)
        {
            throw new NotImplementedException();
        }

        public void Suspend()
        {
            throw new NotImplementedException();
        }
    }
}
