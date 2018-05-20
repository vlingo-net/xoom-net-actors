namespace Vlingo.Actors.Plugin.Logging.NoOp
{
    public class NoOpLoggerProvider : ILoggerProvider
    {
        public NoOpLoggerProvider()
        {
            Logger = new NoOpLogger();
        }

        public ILogger Logger { get; }

        public void Close()
        {
        }
    }
}
