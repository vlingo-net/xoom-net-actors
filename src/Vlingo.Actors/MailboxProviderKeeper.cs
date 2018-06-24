using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    internal sealed class MailboxProviderKeeper
    {
        private readonly IDictionary<string, MailboxProviderInfo> mailboxProviderInfos;

        public MailboxProviderKeeper()
        {
            mailboxProviderInfos = new Dictionary<string, MailboxProviderInfo>();
        }

        internal IMailbox AssignMailbox(string name, int hashCode) =>
            mailboxProviderInfos[name]?.MailboxProvider?.ProvideMailboxFor(hashCode) ??
                throw new KeyNotFoundException($"No registered MailboxProvider named: {name}");

        internal void Close()
        {
            foreach(var info in mailboxProviderInfos.Values)
            {
                info.MailboxProvider.Close();
            }
        }

        internal string FindDefault()
        {
            foreach(var info in mailboxProviderInfos.Values)
            {
                if (info.IsDefault)
                {
                    return info.Name;
                }
            }

            throw new InvalidOperationException("No registered default MailboxProvider.");
        }

        internal void Keep(string name, bool isDefault, IMailboxProvider mailboxProvider)
        {
            if (mailboxProviderInfos.Count == 0)
            {
                isDefault = true;
            }
            else if (isDefault)
            {
                UndefaultCurrentDefault();
            }

            mailboxProviderInfos[name] = new MailboxProviderInfo(name, mailboxProvider, isDefault);
        }

        private void UndefaultCurrentDefault()
        {
            var currentDefaults = mailboxProviderInfos
                .Where(x => x.Value.IsDefault)
                .Select(x => new { x.Key, Info = x.Value })
                .ToList();

            foreach (var item in currentDefaults)
            {
                mailboxProviderInfos[item.Key] = new MailboxProviderInfo(item.Info.Name, item.Info.MailboxProvider, false);
            }
        }

        internal bool IsValidMailboxName(string candidateMailboxName) => mailboxProviderInfos.ContainsKey(candidateMailboxName);
    }

    internal sealed class MailboxProviderInfo
    {
        internal bool IsDefault { get; }
        internal IMailboxProvider MailboxProvider { get; }
        internal string Name { get; }

        internal MailboxProviderInfo(string name, IMailboxProvider mailboxProvider, bool isDefault)
        {
            Name = name;
            MailboxProvider = mailboxProvider;
            IsDefault = isDefault;
        }
    }
}
