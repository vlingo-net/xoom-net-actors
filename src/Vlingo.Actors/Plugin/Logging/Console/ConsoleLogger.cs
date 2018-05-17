using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        public bool IsEnabled => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public static ILogger TestInstance()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Log(string message)
        {
            throw new NotImplementedException();
        }

        public void Log(string message, Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
