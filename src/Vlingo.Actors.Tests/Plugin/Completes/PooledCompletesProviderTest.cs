// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Completes
{
    public class PooledCompletesProviderTest : ActorsTest
    {
        [Fact]
        public void TestActuallyCompletes()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.pooledCompletes", "true");
            properties.SetProperty("plugin.pooledCompletes.classname", "Vlingo.Actors.Plugin.Completes.PooledCompletesPlugin");
            properties.SetProperty("plugin.pooledCompletes.pool", "10");
            
            var pluginProperties = new PluginProperties("pooledCompletes", properties);

            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);

            plugin.Start(World);

            var clientCompletes = new MockCompletes<object>(1);
            var asyncCompletes = World.CompletesFor(clientCompletes);
            asyncCompletes.With(5);
            
            Assert.Equal(1, clientCompletes.WithCount);
            Assert.Equal(5, clientCompletes.Outcome);
        }

        [Fact]
        public void TestCompletesAddressMatches()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.pooledCompletes", "true");
            properties.SetProperty("plugin.pooledCompletes.classname", "Vlingo.Actors.Plugin.Completes.PooledCompletesPlugin");
            properties.SetProperty("plugin.pooledCompletes.pool", "10");

            var pluginProperties = new PluginProperties("pooledCompletes", properties);
            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(World.Configuration, pluginProperties);

            plugin.Start(World);

            var clientCompletes1 = new MockCompletes<object>(1);
            var clientCompletes2 = new MockCompletes<object>(1);
            var completes1 = World.CompletesFor(clientCompletes1);
            completes1.With(5);

            var completes2 = World.CompletesFor(completes1.Address, clientCompletes2);
            completes2.With(10);

            Assert.Equal(1, clientCompletes1.WithCount);
            Assert.Equal(5, clientCompletes1.Outcome);
            Assert.Equal(1, clientCompletes2.WithCount);
            Assert.Equal(10, clientCompletes2.Outcome);
            Assert.Equal(completes1, completes2);
        }
    }
}
