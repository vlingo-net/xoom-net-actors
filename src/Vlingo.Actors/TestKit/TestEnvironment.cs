using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.TestKit;

namespace Vlingo.Actors.TestKit
{
    public class TestEnvironment : Environment
    {
        public TestEnvironment() :
            base(
                TestWorld.testWorld.World.Stage,
                Address.From("test"),
                Definition.Has<Actor>(Definition.NoParameters),
                TestWorld.testWorld.World.DefaultParent,
                new TestMailbox(),
                TestWorld.testWorld.World.DefaultSupervisor,
                ConsoleLogger.TestInstance())
        {
                
        }
    }
}
