using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        internal ConsoleLogger(string name, PluginProperties properties)
        {
            Name = name;
        }

        public bool IsEnabled => true;

        public string Name { get; }

        public static ILogger TestInstance()
        {
            var properties = new Properties();
            var name = "vlingo-net-test";
            return new ConsoleLogger(name, new PluginProperties(name, properties));
        }

        public void Close()
        {
        }

        public void Log(string message)
        {
            System.Console.WriteLine($"{Name}: {message}");
        }

        public void Log(string message, Exception ex)
        {
            System.Console.WriteLine($"{Name}: {message}");
            System.Console.WriteLine($"{Name} [Exception]: {ex.Message}");
            System.Console.WriteLine($"{Name} [StackTrace]: {ex.StackTrace}");
        }
    }
}
