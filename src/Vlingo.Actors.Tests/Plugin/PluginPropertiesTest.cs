// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin
{
    public class PluginPropertiesTest
    {
        [Fact]
        public void TestPropertyTypes()
        {
            var properties = new Properties();

            properties.SetProperty("plugin.test.boolean", "true");
            properties.SetProperty("plugin.test.float", "123.5");
            properties.SetProperty("plugin.test.int", "456");
            properties.SetProperty("plugin.test.string", "text");

            var pluginProperties = new PluginProperties("test", properties);

            Assert.True(pluginProperties.GetBoolean("boolean", false));
            Assert.Equal(123.5f, pluginProperties.GetFloat("float", 0.0f));
            Assert.Equal(456, pluginProperties.GetInteger("int", 0));
            Assert.Equal("text", pluginProperties.GetString("string", ""));
        }

        [Fact]
        public void TestPropertyDefaults()
        {
            var pluginProperties = new PluginProperties("test", new Properties());

            Assert.True(pluginProperties.GetBoolean("boolean", true));
            Assert.Equal(123.5f, pluginProperties.GetFloat("float", 123.5f));
            Assert.Equal(456, pluginProperties.GetInteger("int", 456));
            Assert.Equal("text", pluginProperties.GetString("string", "text"));
        }
    }
}
