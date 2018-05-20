using System;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Logging.NoOp;

namespace Vlingo.Actors
{
    public interface ILogger
    {
        bool IsEnabled { get; }
        string Name { get; }
        void Log(string message);
        void Log(string message, Exception ex);
        void Close();
    }

    public static class Logger
    {
        public static ILogger NoOpLogger() => new NoOpLogger();

        public static ILogger TestLogger() => ConsoleLogger.TestInstance();
    }
}