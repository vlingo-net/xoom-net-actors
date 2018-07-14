using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicLong
    {
        private long value;

        public AtomicLong(long initialValue)
        {
            value = initialValue;
        }

        public long Get() => Interlocked.CompareExchange(ref value, 0, 0);

        public long IncrementAndGet() => Interlocked.Increment(ref value);

        public long GetAndIncrement() => Interlocked.Increment(ref value) - 1;
    }
}
