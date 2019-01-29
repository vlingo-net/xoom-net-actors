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
    public class PooledCompletesProviderTest : IDisposable
    {
        private World world;

        public PooledCompletesProviderTest()
        {
            world = World.Start("test-completes");
        }

        public void Dispose()
        {
            world.Terminate();
        }

        [Fact]
        public void TestActuallyCompletes()
        {
            var properties = new Properties();
            properties.SetProperty("plugin.name.pooledCompletes", "true");
            properties.SetProperty("plugin.pooledCompletes.classname", "Vlingo.Actors.Plugin.Completes.PooledCompletesPlugin");
            properties.SetProperty("plugin.pooledCompletes.pool", "10");
            
            var pluginProperties = new PluginProperties("pooledCompletes", properties);

            var plugin = new PooledCompletesPlugin();
            plugin.Configuration.BuildWith(world.Configuration, pluginProperties);

            plugin.Start(world);

            var clientCompletes = new MockCompletes<object>();
            clientCompletes.UntilWith = TestUntil.Happenings(1);
            var asyncCompletes = world.CompletesFor(clientCompletes);
            asyncCompletes.With(5);
            clientCompletes.UntilWith.Completes();

            Assert.Equal(1, clientCompletes.WithCount);
            Assert.Equal(5, clientCompletes.Outcome);
        }
    }
}
