using System;

namespace Vlingo.Actors.TestKit
{
    public class TestWorld : IDisposable
    {
        public static TestWorld Current;

        public World World { get; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
