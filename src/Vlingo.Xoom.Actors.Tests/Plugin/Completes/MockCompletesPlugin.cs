// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Plugin;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Completes
{
    public class MockCompletesPlugin : IPlugin
    {
        public MockCompletesEventuallyProvider CompletesEventuallyProvider;
        private readonly MockCompletesEventually.CompletesResults _completesResults;
        public MockCompletesPlugin(MockCompletesEventually.CompletesResults completesResults) => _completesResults = completesResults;

        public string Name => null;

        public int Pass => 0;

        public IPluginConfiguration Configuration => null;

        public void Close()
        {
        }

        public void Start(IRegistrar registrar)
        {
            CompletesEventuallyProvider = new MockCompletesEventuallyProvider(_completesResults);
            registrar.Register("mock-completes-eventually", CompletesEventuallyProvider);
        }

        public IPlugin With(IPluginConfiguration overrideConfiguration) => null;
    }
}
