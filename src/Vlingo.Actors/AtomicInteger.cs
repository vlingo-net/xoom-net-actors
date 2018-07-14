using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicInteger
    {
        private int value;

        public AtomicInteger(int initialValue)
        {
            value = initialValue;
        }

        public void Set(int newValue)
        {
            Interlocked.Exchange(ref value, newValue);
        }

        public int Get()
        {
            return Interlocked.CompareExchange(ref value, 0, 0);
        }

        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref value) - 1;
        }
    }
}
