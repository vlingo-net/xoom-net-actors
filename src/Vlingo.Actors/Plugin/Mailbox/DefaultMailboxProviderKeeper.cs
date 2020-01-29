// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors.Plugin.Mailbox
{
    public sealed class DefaultMailboxProviderKeeper : IMailboxProviderKeeper
    {
        private readonly IDictionary<string, MailboxProviderInfo> mailboxProviderInfos;
        private MailboxProviderInfo? defaultProvider;

        public DefaultMailboxProviderKeeper()
        {
            mailboxProviderInfos = new Dictionary<string, MailboxProviderInfo>();
            defaultProvider = null;
        }

        public IMailbox AssignMailbox(string name, int hashCode)
        {
            if (!mailboxProviderInfos.ContainsKey(name))
            {
                throw new InvalidOperationException($"No registered MailboxProvider named: {name}");
            }

            return mailboxProviderInfos[name].mailboxProvider.ProvideMailboxFor(hashCode);
        }

        public void Close()
        {
            foreach(var info in mailboxProviderInfos.Values)
            {
                info.mailboxProvider.Close();
            }
        }

        public string FindDefault()
        {
            if(defaultProvider == null)
            {
                throw new InvalidOperationException("No registered default MailboxProvider.");
            }

            return defaultProvider.name;
        }

        public bool IsValidMailboxName(string candidateMailboxName)
            => mailboxProviderInfos.ContainsKey(candidateMailboxName);

        public void Keep(string name, bool isDefault, IMailboxProvider mailboxProvider)
        {
            var info = new MailboxProviderInfo(name, mailboxProvider);
            mailboxProviderInfos[name] = info;

            if(defaultProvider == null || isDefault)
            {
                defaultProvider = info;
            }
        }

        private sealed class MailboxProviderInfo
        {
            public readonly IMailboxProvider mailboxProvider;
            public readonly string name;

            public MailboxProviderInfo(string name, IMailboxProvider mailboxProvider)
            {
                this.name = name;
                this.mailboxProvider = mailboxProvider;
            }
        }
    }
}
