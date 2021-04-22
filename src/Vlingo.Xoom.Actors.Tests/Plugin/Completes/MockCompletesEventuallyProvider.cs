// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Actors.Tests.Plugin.Completes
{
    public class MockCompletesEventuallyProvider : ICompletesEventuallyProvider
    {
        private readonly MockCompletesEventually.CompletesResults completesResults;
        public int initializeUsing;
        public int provideCompletesForCount;

        public MockCompletesEventually completesEventually;
        public MockCompletes<object> completes;

        public MockCompletesEventuallyProvider(MockCompletesEventually.CompletesResults completesResults)
        {
            this.completesResults = completesResults;
        }

        public ICompletesEventually CompletesEventually => completesEventually;

        public void Close()
        {
        }

        public void InitializeUsing(Stage stage)
        {
            completesEventually = new MockCompletesEventually(completesResults);
            ++initializeUsing;
        }

        public ICompletesEventually ProvideCompletesFor(ICompletes clientCompletes)
        {
            ++provideCompletesForCount;
            return completesEventually;
        }

        public ICompletesEventually ProvideCompletesFor(IAddress address, ICompletes clientCompletes)
        {
            ++provideCompletesForCount;
            return completesEventually;
        }
    }
}
