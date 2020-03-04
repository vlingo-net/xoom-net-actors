// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using NSubstitute;
using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Mailbox;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox
{
    public class DefaultMailboxProviderKeeperPluginTest : ActorsTest
    {
        private readonly IPlugin plugin;
        private readonly IRegistrar registrar;
        private readonly IMailboxProviderKeeper keeper;

        public DefaultMailboxProviderKeeperPluginTest()
            : base()
        {
            registrar = Substitute.For<IRegistrar>();
            keeper = Substitute.For<IMailboxProviderKeeper>();
            plugin = new DefaultMailboxProviderKeeperPlugin(keeper, new DefaultMailboxProviderKeeperPluginConfiguration());
        }

        [Fact]
        public void TestThatItUsesTheCorrectName()
        {
            Assert.Equal("defaultMailboxProviderKeeper", plugin.Name);
        }

        [Fact]
        public void TestThatItsTheFirstPass()
        {
            Assert.Equal(0, plugin.Pass);
        }

        [Fact]
        public void TestThatStartRegistersTheProvidedKeeper()
        {
            plugin.Start(registrar);

            registrar.Received(1)
                .RegisterMailboxProviderKeeper(Arg.Is<IMailboxProviderKeeper>(x => x == keeper));
        }

        [Fact]
        public void TestThatReturnsTheCorrectConfiguration()
        {
            var configuration = plugin.Configuration;
            Assert.Equal(typeof(DefaultMailboxProviderKeeperPluginConfiguration), configuration.GetType());
        }

        [Fact]
        public void TestThatRegistersTheProvidedKeeperInARealWorld()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.defaultMailboxProviderKeeper", "true");
            var pluginProperties = new PluginProperties("defaultMailboxProviderKeeper", properties);
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);
            plugin.Start(World);
            World.Terminate();

            keeper.Received(1)
                .Close();
        }
    }
}
