// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Logging.Console
{
    public class ConsoleLoggerPlugin : AbstractPlugin, ILoggerProvider
    {
        private readonly ConsoleLoggerPluginConfiguration _consoleLoggerPluginConfiguration;
        private int _pass = 1;

        public static ILoggerProvider RegisterStandardLogger(string name, IRegistrar registrar)
        {
            var plugin = new ConsoleLoggerPlugin();
            var pluginConfiguration = (ConsoleLoggerPluginConfiguration)plugin.Configuration;

            var properties = new Properties();
            properties.SetProperty($"plugin.{name}.defaultLogger", "true");

            pluginConfiguration.BuildWith(registrar.World.Configuration, new PluginProperties(name, properties));
            plugin.Start(registrar);

            return plugin;
        }

        public ConsoleLoggerPlugin(string? name = null)
        {
            _consoleLoggerPluginConfiguration = ConsoleLoggerPluginConfiguration.Define();
            Logger = Actors.Logger.NoOpLogger;
        }

        private ConsoleLoggerPlugin(IPluginConfiguration configuration)
        {
            _consoleLoggerPluginConfiguration = (ConsoleLoggerPluginConfiguration)configuration;
            Logger = Actors.Logger.NoOpLogger;
        }

        public override string Name => _consoleLoggerPluginConfiguration.Name;

        public ILogger Logger { get; private set; }

        public override void Close()
        {
            Logger.Close();
        }

        public override int Pass => _pass;

        public override IPluginConfiguration Configuration => _consoleLoggerPluginConfiguration;

        public override void Start(IRegistrar registrar)
        {
            // pass 0 or 1 is bootstrap, pass 2 is for reals
            if (_pass < 2)
            {
                Logger = new ConsoleLogger(_consoleLoggerPluginConfiguration.Name);
                registrar.Register(_consoleLoggerPluginConfiguration.Name, _consoleLoggerPluginConfiguration.IsDefaultLogger, this);
                _pass = 2;
            }
            else if (_pass == 2 && registrar.World != null)
            {
                Logger = registrar.World.ActorFor<ILogger>(Definition.Has<ConsoleLoggerActor>(Definition.Parameters(Logger), Logger));
                registrar.Register(_consoleLoggerPluginConfiguration.Name, _consoleLoggerPluginConfiguration.IsDefaultLogger, this);
            }
        }

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new ConsoleLoggerPlugin(overrideConfiguration);
    }
}
