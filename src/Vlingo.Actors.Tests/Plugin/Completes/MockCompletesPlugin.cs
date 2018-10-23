// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin;

namespace Vlingo.Actors.Tests.Plugin.Completes
{
    public class MockCompletesPlugin : IPlugin
    {
        public MockCompletesEventuallyProvider completesEventuallyProvider;
        private readonly MockCompletesEventually.CompletesResults completesResults;
        public MockCompletesPlugin(MockCompletesEventually.CompletesResults completesResults)
        {
            this.completesResults = completesResults;
        }

        public string Name => null;

        public int Pass => 0;

        public IPluginConfiguration Configuration => null;

        public void Close()
        {
        }

        public void Start(IRegistrar registrar)
        {
            completesEventuallyProvider = new MockCompletesEventuallyProvider(completesResults);
            registrar.Register("mock-completes-eventually", completesEventuallyProvider);
        }
    }
}
