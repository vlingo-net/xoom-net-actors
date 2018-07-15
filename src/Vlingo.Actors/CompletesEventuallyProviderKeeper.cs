// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    internal sealed class CompletesEventuallyProviderKeeper
    {
        private CompletesEventuallyProviderInfo completesEventuallyProviderInfo;

        internal CompletesEventuallyProviderKeeper() { }

        internal ICompletesEventuallyProvider ProviderFor(string name)
        {
            if (completesEventuallyProviderInfo == null)
            {
                throw new InvalidOperationException($"No registered CompletesEventuallyProvider named {name}");
            }
            return completesEventuallyProviderInfo.CompletesEventuallyProvider;
        }

        internal void Close()
        {
            if (completesEventuallyProviderInfo != null)
            {
                completesEventuallyProviderInfo.CompletesEventuallyProvider.Close();
            }
        }

        internal ICompletesEventuallyProvider FindDefault()
        {
            if (completesEventuallyProviderInfo == null)
            {
                throw new InvalidOperationException("No registered default CompletesEventuallyProvider.");
            }
            return completesEventuallyProviderInfo.CompletesEventuallyProvider;
        }

        internal void keep(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            completesEventuallyProviderInfo = new CompletesEventuallyProviderInfo(name, completesEventuallyProvider, true);
        }
    }

    internal sealed class CompletesEventuallyProviderInfo
    {
        internal CompletesEventuallyProviderInfo(
            string name,
            ICompletesEventuallyProvider completesEventuallyProvider,
            bool isDefault)
        {
            Name = name;
            CompletesEventuallyProvider = completesEventuallyProvider;
            IsDefault = isDefault;
        }

        internal string Name { get; }
        internal ICompletesEventuallyProvider CompletesEventuallyProvider { get; }
        internal bool IsDefault { get; }
    }
}
