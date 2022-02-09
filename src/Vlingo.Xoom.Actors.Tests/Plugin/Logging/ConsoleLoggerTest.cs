// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Logging
{
    public class ConsoleLoggerTest
    {
        [Fact]
        public void ConsoleLoggerCreateSuccessfull()
        {
            var consoleLogger = ConsoleLogger.TestInstance();

            Assert.Equal("vlingo-net/actors", consoleLogger.Name);
            Assert.True(consoleLogger.IsEnabled);
        }
    }
}
