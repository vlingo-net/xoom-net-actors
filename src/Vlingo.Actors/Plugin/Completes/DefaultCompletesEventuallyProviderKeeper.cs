// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Completes
{
    public sealed class DefaultCompletesEventuallyProviderKeeper : ICompletesEventuallyProviderKeeper
    {
        private CompletesEventuallyProviderInfo completesEventuallyProviderInfo;

        public void Close()
        {
            completesEventuallyProviderInfo?.completesEventuallyProvider.Close();
        }

        public ICompletesEventuallyProvider FindDefault()
        {
            if (completesEventuallyProviderInfo == null)
            {
                throw new InvalidOperationException("No registered default CompletesEventuallyProvider.");
            }

            return completesEventuallyProviderInfo.completesEventuallyProvider;
        }

        public void Keep(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            completesEventuallyProviderInfo = new CompletesEventuallyProviderInfo(name, completesEventuallyProvider, true);
        }

        public ICompletesEventuallyProvider ProviderFor(string name)
        {
            if (completesEventuallyProviderInfo == null)
            {
                throw new InvalidOperationException($"No registered CompletesEventuallyProvider named: {name}");
            }

            return completesEventuallyProviderInfo.completesEventuallyProvider;
        }

        private class CompletesEventuallyProviderInfo
        {
            public bool isDefault;
            public readonly ICompletesEventuallyProvider completesEventuallyProvider;
            public string name;
            public CompletesEventuallyProviderInfo(string name, ICompletesEventuallyProvider completesEventuallyProvider, bool isDefault)
            {
                this.name = name;
                this.completesEventuallyProvider = completesEventuallyProvider;
                this.isDefault = isDefault;
            }
        }
    }
}
