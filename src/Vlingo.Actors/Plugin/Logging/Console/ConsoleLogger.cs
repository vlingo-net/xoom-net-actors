// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;

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

        public static ILogger BasicInstance()
        {
            var configuration = Configuration.Define();
            var loggerConfiguration = ConsoleLoggerPluginConfiguration.Define();
            loggerConfiguration.Build(configuration);
            return new ConsoleLogger(loggerConfiguration.Name, loggerConfiguration);
        }

        public static ILogger TestInstance() => BasicInstance();

        public void Close()
        {
        }

        public void Debug(string message) => Log(message, "Debug");

        public void Debug(string message, params object[] args) => Log(message, "Debug", args);

        public void Debug(string message, Exception throwable) => Log(message, "Debug", throwable);

        public void Error(string message) => Log(message, "Error");

        public void Error(string message, params object[] args) => Log(message, "Error", args);

        public void Error(string message, Exception throwable) => Log(message, "Error", throwable);

        public void Info(string message) => Log(message, "Info");

        public void Info(string message, params object[] args) => Log(message, "Info", args);

        public void Info(string message, Exception throwable) => Log(message, "Info", throwable);

        public void Trace(string message) => Log(message, "Trace");

        public void Trace(string message, params object[] args) => Log(message, "Trace", args);

        public void Trace(string message, Exception throwable) => Log(message, "Trace", throwable);

        public void Warn(string message) => Log(message, "Warn");

        public void Warn(string message, params object[] args) => Log(message, "Warn", args);

        public void Warn(string message, Exception throwable) => Log(message, "Warn", throwable);

        private void Log(string message, string level)
        {
            System.Console.WriteLine($"{Name}[{level}]: {message}");
        }

        private void Log(string message, string level, Exception ex)
        {
            System.Console.WriteLine($"{Name}[{level}]: {message}");
            System.Console.WriteLine($"{Name}[{level}] [Exception]: {ex.Message}");
            System.Console.WriteLine($"{Name}[{level}] [StackTrace]: {ex.StackTrace}");
        }

        private void Log(string message, string level, params object[] args)
        {
            System.Console.WriteLine($"{Name}[{level}]: {message}");
            var argString = string.Join("\n", args?.Select(x => x.ToString()));
            System.Console.WriteLine($"{Name}[{level}]: args:\n{argString}");
        }
    }
}
