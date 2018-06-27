namespace Vlingo.Actors.TestKit
{
    public class TestActor<T> : ITestStateView
    {
        public TestActor(Actor actor, T protocol, Address address)
        {
            ActorInside = actor;
            Actor = protocol;
            Address = address;
        }

        public T Actor { get; }
        public Address Address { get; }
        public Actor ActorInside { get; }

        public TestState ViewTestState() => ActorInside.ViewTestState();
    }
}
