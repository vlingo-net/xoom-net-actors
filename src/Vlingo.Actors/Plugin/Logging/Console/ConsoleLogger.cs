using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        public static ILogger TestInstance()
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
