// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    internal sealed class LoggerProviderKeeper
    {
        private readonly IDictionary<string, LoggerProviderInfo> loggerProviderInfos;

        internal LoggerProviderKeeper()
        {
            loggerProviderInfos = new Dictionary<string, LoggerProviderInfo>();
        }

        internal void Close()
        {
            foreach (var info in loggerProviderInfos.Values)
            {
                info.LoggerProvider.Close();
            }
        }

        internal ILoggerProvider FindDefault() =>
            loggerProviderInfos
            .Where(x => x.Value.IsDefault)
            .Select(x => x.Value.LoggerProvider)
            .FirstOrDefault();

        internal ILoggerProvider FindNamed(string name) =>
            loggerProviderInfos[name]?.LoggerProvider ?? throw new KeyNotFoundException($"No registered LoggerProvider named: {name}");

        internal void Keep(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            if (loggerProviderInfos.Count == 0)
            {
                isDefault = true;
            }
            else if (isDefault)
            {
                UndefaultCurrentDefault();
            }

            loggerProviderInfos[name] = new LoggerProviderInfo(name, loggerProvider, isDefault);
        }


        private void UndefaultCurrentDefault()
        {
            var currentDefaults = loggerProviderInfos
                .Where(x => x.Value.IsDefault)
                .Select(x => new { x.Key, Info = x.Value })
                .ToList();

            foreach(var item in currentDefaults)
            {
                loggerProviderInfos[item.Key] = new LoggerProviderInfo(item.Info.Name, item.Info.LoggerProvider, false);
            }
        }
    }


    internal sealed class LoggerProviderInfo
    {
        internal bool IsDefault { get; }
        internal ILoggerProvider LoggerProvider { get; }
        internal string Name { get; }

        internal LoggerProviderInfo(string name, ILoggerProvider loggerProvider, bool isDefault)
        {
            Name = name;
            LoggerProvider = loggerProvider;
            IsDefault = isDefault;
        }
    }
}
