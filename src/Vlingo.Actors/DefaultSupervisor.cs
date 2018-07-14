using System;

namespace Vlingo.Actors
{
    public abstract class DefaultSupervisor : Actor, ISupervisor
    {
        internal static readonly ISupervisionStrategy DefaultSupervisionStrategy = new DefaultSupervisionStrategyImpl();

        internal DefaultSupervisor() { }

        public ISupervisionStrategy SupervisionStrategy => DefaultSupervisionStrategy;

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            supervised.RestartWithin(
                DefaultSupervisionStrategy.Period,
                DefaultSupervisionStrategy.Intensity,
                DefaultSupervisionStrategy.Scope);
        }
    }
}
