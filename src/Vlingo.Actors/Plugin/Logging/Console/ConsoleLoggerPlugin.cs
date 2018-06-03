using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLoggerPlugin : IPlugin, ILoggerProvider
    {

        public static ILoggerProvider RegisterStandardLogger(string name, IRegistrar registrar)
        {
            var properties = new Properties();
            properties.SetProperty("plugin.jdkLogger.defaulLogger", "true");
            properties.SetProperty("plugin.jdkLogger.handler.classname", "io.vlingo.actors.plugin.logging.jdk.DefaultHandler");
            properties.SetProperty("plugin.jdkLogger.handler.name", name);
            properties.SetProperty("plugin.jdkLogger.handler.level", "ALL");

            var plugin = new ConsoleLoggerPlugin();
            plugin.Start(registrar, name, new PluginProperties("ConsoleLogger", properties));

            return plugin;
        }

        public string Name { get; private set; }

        public ILogger Logger { get; private set; }

        public void Close()
        {
            Logger.Close();
        }

        public int Pass => 1;

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = properties.GetString("name", name);
            Logger = new ConsoleLogger(Name, properties);
            var isDefaultLogger = properties.GetBoolean("defaultLogger", true);
            registrar.Register(name, isDefaultLogger, this);
        }
    }
}
