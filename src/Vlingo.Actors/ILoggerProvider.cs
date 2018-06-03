using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Logging.NoOp;

namespace Vlingo.Actors
{
    public interface ILoggerProvider
    {
        void Close();
        ILogger Logger { get; }
    }

    public static class LoggerProvider
    {
        public static ILoggerProvider NoOpLoggerProvider() => new NoOpLoggerProvider();

        public static ILoggerProvider StandardLoggerProvider(World world, string name) =>
            ConsoleLoggerPlugin.RegisterStandardLogger(name, world);
    }
}
