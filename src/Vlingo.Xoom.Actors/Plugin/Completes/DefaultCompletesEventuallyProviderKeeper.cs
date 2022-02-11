// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Completes;

public sealed class DefaultCompletesEventuallyProviderKeeper : ICompletesEventuallyProviderKeeper
{
    private CompletesEventuallyProviderInfo? _completesEventuallyProviderInfo;

    public void Close() => _completesEventuallyProviderInfo?.CompletesEventuallyProvider.Close();

    public ICompletesEventuallyProvider FindDefault()
    {
        if (_completesEventuallyProviderInfo == null)
        {
            throw new InvalidOperationException("No registered default CompletesEventuallyProvider.");
        }

        return _completesEventuallyProviderInfo.CompletesEventuallyProvider;
    }

    public void Keep(string name, ICompletesEventuallyProvider completesEventuallyProvider) => _completesEventuallyProviderInfo = new CompletesEventuallyProviderInfo(name, completesEventuallyProvider, true);

    public ICompletesEventuallyProvider ProviderFor(string name)
    {
        if (_completesEventuallyProviderInfo == null)
        {
            throw new InvalidOperationException($"No registered CompletesEventuallyProvider named: {name}");
        }

        return _completesEventuallyProviderInfo.CompletesEventuallyProvider;
    }

    private class CompletesEventuallyProviderInfo
    {
        public bool IsDefault;
        public readonly ICompletesEventuallyProvider CompletesEventuallyProvider;
        public string Name;
        public CompletesEventuallyProviderInfo(string name, ICompletesEventuallyProvider completesEventuallyProvider, bool isDefault)
        {
            Name = name;
            CompletesEventuallyProvider = completesEventuallyProvider;
            IsDefault = isDefault;
        }
    }
}