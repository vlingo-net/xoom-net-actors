// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Logging;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Logging.NoOp;

namespace Vlingo.Actors
{
    public interface ILogger
    {
        bool IsEnabled { get; }
        string Name { get; }
        void Close();

        void Trace(string message);
        void Trace(string message, params object[] args);
        void Trace(string message, Exception exception);

        void Debug(string message);
        void Debug(string message, params object[] args);
        void Debug(string message, Exception exception);

        void Info(string message);
        void Info(string message, params object[] args);
        void Info(string message, Exception exception);

        void Warn(string message);
        void Warn(string message, params object[] args);
        void Warn(string message, Exception exception);

        void Error(string message);
        void Error(string message, params object[] args);
        void Error(string message, Exception exception);
        
        void Trace(LogEvent logEvent);
        void Debug(LogEvent logEvent);
        void Info(LogEvent logEvent);
        void Warn(LogEvent logEvent);
        void Error(LogEvent logEvent);
    }

    internal static class Logger
    {
        public static ILogger NoOpLogger => new NoOpLogger();

        public static ILogger BasicLogger => ConsoleLogger.BasicInstance();
    }
}