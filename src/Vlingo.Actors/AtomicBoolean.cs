using System.Threading;

namespace Vlingo.Actors
{
    public class AtomicBoolean
    {
        private int value;

        public AtomicBoolean(bool initialValue)
        {
            value = initialValue ? 1 : 0;
        }

        public bool Get()
        {
            return Interlocked.CompareExchange(ref value, 0, 0) == 1;
        }

        public void Set(bool update)
        {
            var updateInt = update ? 1 : 0;
            Interlocked.Exchange(ref value, updateInt);
        }

        public bool CompareAndSet(bool expect, bool update)
        {
            var expectedInt = expect ? 1 : 0;
            var updateInt = update ? 1 : 0;

            var actualInt = Interlocked.CompareExchange(ref value, updateInt, expectedInt);

            return expectedInt == actualInt;
        }
    }
}
