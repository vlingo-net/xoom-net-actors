// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLoggerPluginConfiguration : IPluginConfiguration
    {
        private ConsoleLoggerPluginConfiguration()
        {
            Name = "consoleLogger";
        }

        public static ConsoleLoggerPluginConfiguration Define() => new ConsoleLoggerPluginConfiguration();

        public ConsoleLoggerPluginConfiguration WithDefaultLogger()
        {
            IsDefaultLogger = true;
            return this;
        }

        public ConsoleLoggerPluginConfiguration WithName(string name)
        {
            Name = name;
            return this;
        }

        public bool IsDefaultLogger { get; private set; }

        public string Name { get; private set; }

        public void Build(Configuration configuration)
        {
            configuration.With(
                WithDefaultLogger()
                .WithName("vlingo-net/actors(test)")
            );
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            Name = properties.Name;
            IsDefaultLogger = properties.GetBoolean("defaultLogger", true);
        }
    }
}
