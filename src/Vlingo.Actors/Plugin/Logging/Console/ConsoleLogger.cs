// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        internal ConsoleLogger(string name, ConsoleLoggerPluginConfiguration configuration)
        {
            Name = name;
        }

        public bool IsEnabled => true;

        public string Name { get; }

        public static ILogger TestInstance()
        {
            var configuration = Configuration.Define();
            var loggerConfiguration = ConsoleLoggerPluginConfiguration.Define();
            loggerConfiguration.Build(configuration);
            return new ConsoleLogger(loggerConfiguration.Name, loggerConfiguration);
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
