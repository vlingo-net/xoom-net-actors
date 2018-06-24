using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Actors.TestKit
{
    public class TestEnvironment : Environment
    {
        public TestEnvironment() :
            base(
                TestWorld.Current.World.Stage,
                Address.From("test"),
                Definition.Has<Actor>(Definition.NoParameters),
                TestWorld.Current.World.DefaultParent,
                new TestMailbox(),
                TestWorld.Current.World.DefaultSupervisor,
                ConsoleLogger.TestInstance())
        {
                
        }
    }
}
