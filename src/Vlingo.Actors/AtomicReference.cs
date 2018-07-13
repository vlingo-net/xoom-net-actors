using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicReference<T> where T : class
    {
        private T value;
        private readonly T defaultValue;

        public AtomicReference(T initialValue)
        {
            value = initialValue;
            defaultValue = default(T);
        }

        public AtomicReference()
            : this(default(T))
        {
        }

        public T Get() => Interlocked.CompareExchange<T>(ref value, defaultValue, defaultValue);

        public void Set(T newValue) => Interlocked.Exchange<T>(ref value, newValue);
    }
}
