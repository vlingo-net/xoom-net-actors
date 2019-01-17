// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
        }

        public void Close() => logger.Close();

        public bool IsEnabled => logger.IsEnabled;

        public string Name => logger.Name;

        public void Log(string message) => logger.Log(message);

        public void Log(string message, Exception ex) => logger.Log(message, ex);

        public override void Stop()
        {
            Close();
            base.Stop();
        }
    }
}
