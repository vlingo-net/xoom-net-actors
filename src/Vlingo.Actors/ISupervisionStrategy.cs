using static Vlingo.Actors.SupervisionStrategyConstants;

namespace Vlingo.Actors
{
    public interface ISupervisionStrategy
    {
        int Intensity { get; }
        long Period { get; }
        Scope Scope { get; }
    }

    public static class SupervisionStrategyConstants
    {
        public enum Scope { All, One };
        public const int DefaultIntensity = 1;
        public const int ForeverIntensity = -1;
        public const long DefaultPeriod = 5000;
        public const long ForeverPeriod = long.MaxValue;

    }

    internal sealed class DefaultSupervisionStrategyImpl : ISupervisionStrategy
    {
        public int Intensity => DefaultIntensity;

        public long Period => DefaultPeriod;

        public Scope Scope => SupervisionStrategyConstants.Scope.One;
    }
}
