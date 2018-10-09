// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLoggerPlugin : IPlugin, ILoggerProvider
    {
        private readonly ConsoleLoggerPluginConfiguration consoleLoggerPluginConfiguration;
        private int pass = 0;

        public static ILoggerProvider RegisterStandardLogger(string name, IRegistrar registrar)
        {
            var plugin = new ConsoleLoggerPlugin();
            var pluginConfiguration = (ConsoleLoggerPluginConfiguration)plugin.Configuration;

            var properties = new Properties();
            properties.SetProperty("plugin.consoleLogger.defaulLogger", "true");

            pluginConfiguration.BuildWith(registrar.World.Configuration, new PluginProperties(name, properties));
            plugin.Start(registrar);

            return plugin;
        }

        public ConsoleLoggerPlugin()
        {
            consoleLoggerPluginConfiguration = ConsoleLoggerPluginConfiguration.Define();
        }

        public string Name => consoleLoggerPluginConfiguration.Name;

        public ILogger Logger { get; private set; }

        public void Close()
        {
            Logger.Close();
        }

        public int Pass
        {
            get
            {
                pass = pass == 0 ? 1 : 2;
                return pass;
            }
        }

        public IPluginConfiguration Configuration => consoleLoggerPluginConfiguration;

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            // pass 0 or 1 is bootstrap, pass 2 is for reals
            if (pass < 2)
            {
                Logger = new ConsoleLogger(consoleLoggerPluginConfiguration.Name, consoleLoggerPluginConfiguration);
                registrar.Register(consoleLoggerPluginConfiguration.Name, consoleLoggerPluginConfiguration.IsDefaultLogger, this);
            }
            else if (pass == 2 && registrar.World != null)
            {
                Logger = registrar.World.ActorFor<ILogger>(Definition.Has<ConsoleLoggerActor>(Definition.Parameters(Logger), Logger));
                registrar.Register(consoleLoggerPluginConfiguration.Name, consoleLoggerPluginConfiguration.IsDefaultLogger, this);
            }
        }

        public void Start(IRegistrar registrar)
        {
            throw new System.NotImplementedException();
        }
    }
}
