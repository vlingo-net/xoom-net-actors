// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using NSubstitute;
using Vlingo.Xoom.Actors.Plugin;
using Vlingo.Xoom.Actors.Plugin.Mailbox;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox
{
    public class DefaultMailboxProviderKeeperPluginTest : ActorsTest
    {
        private readonly IPlugin _plugin;
        private readonly IRegistrar _registrar;
        private readonly IMailboxProviderKeeper _keeper;

        public DefaultMailboxProviderKeeperPluginTest()
            : base()
        {
            _registrar = Substitute.For<IRegistrar>();
            _keeper = Substitute.For<IMailboxProviderKeeper>();
            _plugin = new DefaultMailboxProviderKeeperPlugin(_keeper, new DefaultMailboxProviderKeeperPluginConfiguration());
        }

        [Fact]
        public void TestThatItUsesTheCorrectName()
        {
            Assert.Equal("defaultMailboxProviderKeeper", _plugin.Name);
        }

        [Fact]
        public void TestThatItsTheFirstPass()
        {
            Assert.Equal(0, _plugin.Pass);
        }

        [Fact]
        public void TestThatStartRegistersTheProvidedKeeper()
        {
            _plugin.Start(_registrar);

            _registrar.Received(1)
                .RegisterMailboxProviderKeeper(Arg.Is<IMailboxProviderKeeper>(x => x == _keeper));
        }

        [Fact]
        public void TestThatReturnsTheCorrectConfiguration()
        {
            var configuration = _plugin.Configuration;
            Assert.Equal(typeof(DefaultMailboxProviderKeeperPluginConfiguration), configuration.GetType());
        }

        [Fact]
        public void TestThatRegistersTheProvidedKeeperInARealWorld()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.defaultMailboxProviderKeeper", "true");
            var pluginProperties = new PluginProperties("defaultMailboxProviderKeeper", properties);
            _plugin.Configuration.BuildWith(World.Configuration, pluginProperties);
            _plugin.Start(World);
            World.Terminate();

            _keeper.Received(1)
                .Close();
        }
    }
}
