// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Threading;

namespace Vlingo.Xoom.Actors.Plugin.Logging.Console
{
    public class ConsoleLogger : ILogger
    {
        internal ConsoleLogger(string name)
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
            return new ConsoleLogger(loggerConfiguration.Name);
        }

        public static ILogger TestInstance() => BasicInstance();

        public void Close()
        {
        }

        public void Debug(string message) => Log(message, "Debug");

        public void Debug(string message, params object[] args) => Log(message, "Debug", args);

        public void Debug(string message, Exception exception) => Log(message, "Debug", exception);

        public void Error(string message) => Log(message, "Error");

        public void Error(string message, params object[] args) => Log(message, "Error", args);

        public void Error(string message, Exception exception) => Log(message, "Error", exception);

        public void Info(string message) => Log(message, "Info");

        public void Info(string message, params object[] args) => Log(message, "Info", args);

        public void Info(string message, Exception exception) => Log(message, "Info", exception);

        public void Trace(string message) => Log(message, "Trace");

        public void Trace(string message, params object[] args) => Log(message, "Trace", args);

        public void Trace(string message, Exception exception) => Log(message, "Trace", exception);

        public void Warn(string message) => Log(message, "Warn");

        public void Warn(string message, params object[] args) => Log(message, "Warn", args);

        public void Warn(string message, Exception exception) => Log(message, "Warn", exception);

        public void Trace(LogEvent logEvent) => Log("Trace", logEvent);

        public void Debug(LogEvent logEvent) => Log("Debug", logEvent);

        public void Info(LogEvent logEvent) => Log("Info", logEvent);

        public void Warn(LogEvent logEvent) => Log("Warn", logEvent);

        public void Error(LogEvent logEvent) => Log("Error", logEvent);

        private void Log(string level, LogEvent logEvent)
        {
            System.Console.ForegroundColor = ColorFor(level);
            System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: {logEvent.Source}");
            if (logEvent.SourceActorAddress != null)
            {
                System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: {logEvent.SourceActorAddress}");
            }

            System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: {logEvent.Message}");

            if (logEvent.Args != null)
            {
                System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: {logEvent.Args.Select(x => x.ToString())}");
            }

            if (logEvent.Exception != null)
            {
                System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: [Exception]: {logEvent.Exception.Message}");
                System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: [StackTrace]: {logEvent.Exception.StackTrace}");
                if (logEvent.Exception.InnerException != null)
                {
                    System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: [InnerException]: {logEvent.Exception.InnerException.Message}");
                    System.Console.WriteLine($"{logEvent.EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff} [{logEvent.SourceThread}] {Name}[{level}]: [InnerException-StackTrace]: {logEvent.Exception.InnerException.StackTrace}");
                }
            }
            
            System.Console.ResetColor();
        }

        private void Log(string message, string level)
        {
            System.Console.ForegroundColor = ColorFor(level);
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}]: {message}");
            System.Console.ResetColor();
        }

        private void Log(string message, string level, Exception ex)
        {
            System.Console.ForegroundColor = ColorFor(level);
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}]: {message}");
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}] [Exception]: {ex.Message}");
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}] [StackTrace]: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                var iex = ex.InnerException;
                System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}] [InnerException]: {iex.Message}");
                System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}] [InnerException-StackTrace]: {iex.StackTrace}");
            }
            System.Console.ResetColor();
        }

        private void Log(string message, string level, params object[] args)
        {
            System.Console.ForegroundColor = ColorFor(level);
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}]: {message}");
            var argString = string.Join("\n", args?.Select(x => x.ToString())!);
            System.Console.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} [{Thread.CurrentThread.ManagedThreadId}] {Name}[{level}]: args:\n{argString}");
            System.Console.ResetColor();
        }

        private ConsoleColor ColorFor(string level)
        {
            var defaultColor = System.Console.ForegroundColor;
            switch (level)
            {
                case "Debug":
                    return ConsoleColor.Gray;
                case "Trace":
                    return ConsoleColor.White;
                case "Info":
                    return ConsoleColor.Blue;
                case "Warn":
                    return ConsoleColor.Yellow;
                case "Error":
                    return ConsoleColor.Red;
                default:
                    return defaultColor;
            }
        }
    }
}
