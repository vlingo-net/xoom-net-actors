namespace Vlingo.Actors
{
    public interface IScheduled
    {
        void IntervalSignal(IScheduled scheduled, object data);
    }
}
