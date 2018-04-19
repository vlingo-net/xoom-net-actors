namespace Vlingo.Actors
{
    public interface IStoppable
    {
        bool IsStopped { get; }
        void Stop();
    }
}