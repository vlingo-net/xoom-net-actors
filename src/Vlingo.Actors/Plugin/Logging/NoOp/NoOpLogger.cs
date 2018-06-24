using System;

namespace Vlingo.Actors.Plugin.Logging.NoOp
{
    public class NoOpLogger : ILogger
    {
        public bool IsEnabled => false;

        public string Name => "no-op";

        public void Close()
        {
        }

        public void Log(string message)
        {
        }

        public void Log(string message, Exception ex)
        {
        }
    }
}
