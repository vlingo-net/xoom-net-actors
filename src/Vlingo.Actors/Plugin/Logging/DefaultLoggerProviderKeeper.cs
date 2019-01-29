// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors.Plugin.Logging
{
    public sealed class DefaultLoggerProviderKeeper : ILoggerProviderKeeper
    {
        private readonly IDictionary<string, LoggerProviderInfo> loggerProviderInfos;

        public DefaultLoggerProviderKeeper()
        {
            loggerProviderInfos = new Dictionary<string, LoggerProviderInfo>();
        }

        public void Close()
        {
            foreach (var info in loggerProviderInfos.Values)
            {
                info.loggerProvider.Close();
            }
        }

        public ILoggerProvider FindDefault()
            => loggerProviderInfos.Values
                .Where(info => info.isDefault)
                .Select(info => info.loggerProvider)
                .FirstOrDefault();

        public ILoggerProvider FindNamed(string name)
        {
            if (loggerProviderInfos.ContainsKey(name))
            {
                return loggerProviderInfos[name].loggerProvider;
            }

            throw new InvalidOperationException($"No registered LoggerProvider named: {name}");
        }

        public void Keep(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            if (loggerProviderInfos.Count == 0 || FindDefault() == null)
            {
                isDefault = true;
            }

            if (isDefault)
            {
                UndefaultCurrentDefault();
            }

            loggerProviderInfos[name] = new LoggerProviderInfo(name, loggerProvider, isDefault);
        }

        private void UndefaultCurrentDefault()
        {
            var defaultItems = loggerProviderInfos.Where(x => x.Value.isDefault).ToList();
            defaultItems
                .ForEach(item =>
                {
                    loggerProviderInfos[item.Key] = new LoggerProviderInfo(item.Value.name, item.Value.loggerProvider, false);
                });
        }

        private class LoggerProviderInfo
        {
            public readonly bool isDefault;
            public readonly ILoggerProvider loggerProvider;
            public readonly string name;
            public LoggerProviderInfo(string name, ILoggerProvider loggerProvider, bool isDefault)
            {
                this.name = name;
                this.loggerProvider = loggerProvider;
                this.isDefault = isDefault;
            }
        }
    }
}
