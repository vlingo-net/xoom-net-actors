// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox;

public sealed class DefaultMailboxProviderKeeper : IMailboxProviderKeeper
{
    private readonly IDictionary<string, MailboxProviderInfo> _mailboxProviderInfos;
    private MailboxProviderInfo? _defaultProvider;

    public DefaultMailboxProviderKeeper()
    {
        _mailboxProviderInfos = new Dictionary<string, MailboxProviderInfo>();
        _defaultProvider = null;
    }

    public IMailbox AssignMailbox(string name, int? hashCode)
    {
        if (!_mailboxProviderInfos.ContainsKey(name))
        {
            throw new InvalidOperationException($"No registered MailboxProvider named: {name}");
        }

        return _mailboxProviderInfos[name].MailboxProvider.ProvideMailboxFor(hashCode);
    }

    public void Close()
    {
        foreach(var info in _mailboxProviderInfos.Values)
        {
            info.MailboxProvider.Close();
        }
    }

    public string FindDefault()
    {
        if (_defaultProvider == null)
        {
            throw new InvalidOperationException("No registered default MailboxProvider.");
        }

        return _defaultProvider.Name;
    }

    public bool IsValidMailboxName(string candidateMailboxName)
        => _mailboxProviderInfos.ContainsKey(candidateMailboxName);

    public void Keep(string name, bool isDefault, IMailboxProvider mailboxProvider)
    {
        var info = new MailboxProviderInfo(name, mailboxProvider);
        _mailboxProviderInfos[name] = info;

        if (_defaultProvider == null || isDefault)
        {
            _defaultProvider = info;
        }
    }

    private sealed class MailboxProviderInfo
    {
        public readonly IMailboxProvider MailboxProvider;
        public readonly string Name;

        public MailboxProviderInfo(string name, IMailboxProvider mailboxProvider)
        {
            Name = name;
            MailboxProvider = mailboxProvider;
        }
    }
}