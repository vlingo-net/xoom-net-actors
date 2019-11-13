using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Actors.Plugin.Logging.Console;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Logging
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
