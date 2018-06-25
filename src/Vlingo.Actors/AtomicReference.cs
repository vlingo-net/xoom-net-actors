using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicReference<T> where T : class
    {
        private T value;

        public AtomicReference(T initialValue)
        {
            value = initialValue;
        }

        public AtomicReference()
            : this(default(T))
        {
        }

        public T Get() => Volatile.Read<T>(ref value);

        public void Set(T newValue) => Volatile.Write<T>(ref value, newValue);
    }
}
