using System;

namespace Vlingo.Actors.Plugin.Supervision
{
    public class DefaultSupervisorOverride : Actor, ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy { get; } = new DefaultSupervisorOvveride_SupervisionStrategy();

        public ISupervisor Supervisor { get; private set; }

        public DefaultSupervisorOverride()
        {
            Supervisor = Stage.ActorAs<ISupervisor>(Parent);
        }

        public void Inform(Exception error, ISupervised supervised)
        {
            Logger.Log($"DefaultSupervisorOverride: Failure of: {supervised.Address}", error);
            supervised.Resume();
        }

        private class DefaultSupervisorOvveride_SupervisionStrategy : ISupervisionStrategy
        {
            public int Intensity => SupervisionStrategyConstants.ForeverIntensity;

            public long Period => SupervisionStrategyConstants.ForeverPeriod;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
