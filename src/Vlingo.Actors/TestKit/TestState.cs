using System.Collections.Generic;

namespace Vlingo.Actors.TestKit
{
    public class TestState
    {
        private readonly IDictionary<string, object> state;

        public TestState()
        {
            state = new Dictionary<string, object>();
        }

        public TestState PutValue(string name, object value)
        {
            state[name] = value;
            return this;
        }

        public T ValueOf<T>(string name)
        {
            return (T)state[name];
        }
    }
}
