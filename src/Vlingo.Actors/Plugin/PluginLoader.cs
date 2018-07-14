using System;
using System.Collections.Generic;
using System.IO;

namespace Vlingo.Actors.Plugin
{
    public class PluginLoader
    {
        private const string PropertiesFile = "/vlingo-actors.properties";
        private const string PluginNamePrefix = "plugin.name.";

        private readonly IDictionary<string, IPlugin> plugins;

        public PluginLoader()
        {
            plugins = new Dictionary<string, IPlugin>();
        }

        public void LoadEnabledPlugins(IRegistrar registrar, int pass)
            => LoadEnabledPlugins(registrar, pass, false);

        public void LoadEnabledPlugins(IRegistrar registrar, int pass, bool forceDefaults)
        {
            var properties = forceDefaults ? LoadDefaultProperties() : LoadProperties();

            foreach(var enabledPlugin in FindEnabledPlugins(properties))
            {
                RegisterPlugin(registrar, properties, enabledPlugin, pass);
            }
        }

        private ISet<string> FindEnabledPlugins(Properties properties)
        {
            var enabledPlugins = new HashSet<string>();
            foreach(var key in properties.Keys)
            {
                if (key.StartsWith(PluginNamePrefix))
                {
                    if (bool.TryParse(properties.GetProperty(key), out bool _))
                    {
                        enabledPlugins.Add(key);
                    }
                }
            }

            return enabledPlugins;
        }

        private Properties LoadDefaultProperties()
        {
            var properties = new Properties();
            SetUpDefaulted(properties);
            return properties;
        }

        private Properties LoadProperties()
        {
            var properties = new Properties();
            try
            {
                properties.Load(new FileInfo(PropertiesFile));
            }
            catch (Exception)
            {
                SetUpDefaulted(properties);
            }
            
            return properties;
        }

        private void RegisterPlugin(IRegistrar registrar, Properties properties, string enabledPlugin, int pass)
        {
            var pluginName = enabledPlugin.Substring(PluginNamePrefix.Length);
            var classNameKey = $"plugin.{pluginName}.classname";
            var className = properties.GetProperty(classNameKey);
            try
            {
                var plugin = PluginOf(className);
                if(plugin.Pass == pass)
                {
                    plugin.Start(registrar, pluginName, new PluginProperties(pluginName, properties));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new ArgumentException($"Could not load plugin {className}");
            }
        }

        private IPlugin PluginOf(string className)
        {
            if (plugins.ContainsKey(className))
            {
                return plugins[className];
            }

            var pluginClass = Type.GetType(className, true, true);
            var plugin = (IPlugin)Activator.CreateInstance(pluginClass);
            plugins[className] = plugin;

            return plugin;
        }

        private void SetUpDefaulted(Properties properties)
        {
            properties.SetProperty("plugin.name.pooledCompletes", "true");
            properties.SetProperty("plugin.pooledCompletes.classname", "Vlingo.Actors.Plugin.Completes.PooledCompletesPlugin");
            properties.SetProperty("plugin.pooledCompletes.pool", "10");

            properties.SetProperty("plugin.name.queueMailbox", "true");
            properties.SetProperty("plugin.queueMailbox.classname", "Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue.ConcurrentQueueMailboxPlugin");
            properties.SetProperty("plugin.queueMailbox.defaultMailbox", "true");
            properties.SetProperty("plugin.queueMailbox.numberOfDispatchersFactor", "1.5");
            properties.SetProperty("plugin.queueMailbox.dispatcherThrottlingCount", "10");

            properties.SetProperty("plugin.name.consoleLogger", "true");
            properties.SetProperty("plugin.consoleLogger.classname", "Vlingo.Actors.Plugin.Logging.Console.ConsoleLoggerPlugin");
            properties.SetProperty("plugin.consoleLogger.name", "vlingo-net/actors");
            properties.SetProperty("plugin.consoleLogger.defaultLogger", "false");
        }
    }
}
