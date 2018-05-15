using System;
using static Vlingo.Actors.SupervisionStrategyConstants;

namespace Vlingo.Actors
{
    public interface ISupervised
    {
        Address Address { get; }
        void Escalate();
        void RestartWithin(long period, int intensity, Scope scope);
        void Resume();
        void Stop(Scope scope);
        ISupervisor Supervisor { get; }
        void Suspend();
        Exception Error { get; }
    }
}