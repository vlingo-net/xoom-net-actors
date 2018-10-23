// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Logging.NoOp;

namespace Vlingo.Actors
{
    public interface ILogger
    {
        bool IsEnabled { get; }
        string Name { get; }
        void Log(string message);
        void Log(string message, Exception ex);
        void Close();
    }

    public static class Logger
    {
        public static ILogger NoOpLogger() => new NoOpLogger();

        public static ILogger TestLogger() => ConsoleLogger.TestInstance();
    }
}