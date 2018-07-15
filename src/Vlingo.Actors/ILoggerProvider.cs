// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Logging.NoOp;

namespace Vlingo.Actors
{
    public interface ILoggerProvider
    {
        void Close();
        ILogger Logger { get; }
    }

    public static class LoggerProvider
    {
        public static ILoggerProvider NoOpLoggerProvider() => new NoOpLoggerProvider();

        public static ILoggerProvider StandardLoggerProvider(World world, string name) =>
            ConsoleLoggerPlugin.RegisterStandardLogger(name, world);
    }
}
