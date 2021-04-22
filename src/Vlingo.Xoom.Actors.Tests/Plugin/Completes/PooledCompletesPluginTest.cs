// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Completes
{
    public class PooledCompletesPluginTest
    {
        [Fact]
        public void TestStart()
        {
            var completesResults = new MockCompletesEventually.CompletesResults();
            var plugin = new MockCompletesPlugin(completesResults);
            var registrar = new MockRegistrar();
            plugin.Start(registrar);
            plugin.CompletesEventuallyProvider.completesEventually.With(new object());
            var completes = (MockCompletesEventually)plugin.CompletesEventuallyProvider.ProvideCompletesFor(null);

            completes.With(7);

            Assert.Equal(1, registrar.RegisterCount);
            Assert.Equal(1, plugin.CompletesEventuallyProvider.initializeUsing);
            Assert.Equal(1, plugin.CompletesEventuallyProvider.provideCompletesForCount);
            Assert.Equal(2, completesResults.WithCount.Get());
            Assert.Equal(2, completes.completesResults.WithCount.Get());
            Assert.Equal(7, completes.completesResults.Outcome.Get());
        }
    }
}
