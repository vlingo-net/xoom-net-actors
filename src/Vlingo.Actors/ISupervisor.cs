using System;

namespace Vlingo.Actors
{
    public interface ISupervisor
    {
        void Inform(Exception error, ISupervised supervised);
        ISupervisionStrategy SupervisionStrategy { get; }
        ISupervisor Supervisor { get; }
    }

    internal sealed class DefaultSupervisorImpl : ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy => DefaultSupervisor.DefaultSupervisionStrategy;

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public void Inform(Exception error, ISupervised supervised)
        {
            var strategy = DefaultSupervisor.DefaultSupervisionStrategy;
            supervised.RestartWithin(strategy.Period, strategy.Intensity, strategy.Scope);
        }
    }

}