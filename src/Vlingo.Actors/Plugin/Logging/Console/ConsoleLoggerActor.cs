// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Logging.Console
{
    public class ConsoleLoggerActor : Actor, ILogger
    {
        private readonly ConsoleLogger logger;

        public ConsoleLoggerActor(ConsoleLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Close() => logger.Close();

        public bool IsEnabled => logger.IsEnabled;

        public string Name => logger.Name;

        public override void Stop()
        {
            Close();
            base.Stop();
        }

        public void Trace(string message) => logger.Trace(message);

        public void Trace(string message, params object[] args) => logger.Trace(message, args);

        public void Trace(string message, Exception throwable) => logger.Trace(message, throwable);

        public void Debug(string message) => logger.Debug(message);

        public void Debug(string message, params object[] args) => logger.Debug(message, args);

        public void Debug(string message, Exception throwable) => logger.Debug(message, throwable);

        public void Info(string message) => logger.Info(message);

        public void Info(string message, params object[] args) => logger.Info(message, args);

        public void Info(string message, Exception throwable) => logger.Info(message, throwable);

        public void Warn(string message) => logger.Warn(message);

        public void Warn(string message, params object[] args) => logger.Warn(message, args);

        public void Warn(string message, Exception throwable) => logger.Warn(message, throwable);

        public void Error(string message) => logger.Error(message);

        public void Error(string message, params object[] args) => logger.Error(message, args);

        public void Error(string message, Exception throwable) => logger.Error(message, throwable);
    }
}
