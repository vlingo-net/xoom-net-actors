// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
        private readonly IDictionary<string, LoggerProviderInfo> _loggerProviderInfos;

        public DefaultLoggerProviderKeeper() => _loggerProviderInfos = new Dictionary<string, LoggerProviderInfo>();

        public void Close()
        {
            foreach (var info in _loggerProviderInfos.Values)
            {
                info.LoggerProvider.Close();
            }
        }

        public ILoggerProvider? FindDefault()
            => _loggerProviderInfos.Values
                .Where(info => info.IsDefault)
                .Select(info => info.LoggerProvider)
                .FirstOrDefault();

        public ILoggerProvider FindNamed(string name)
        {
            if (_loggerProviderInfos.ContainsKey(name))
            {
                return _loggerProviderInfos[name].LoggerProvider;
            }

            throw new InvalidOperationException($"No registered LoggerProvider named: {name}");
        }

        public void Keep(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            if (_loggerProviderInfos.Count == 0 || FindDefault() == null)
            {
                isDefault = true;
            }

            if (isDefault)
            {
                UndefaultCurrentDefault();
            }

            _loggerProviderInfos[name] = new LoggerProviderInfo(name, loggerProvider, isDefault);
        }

        private void UndefaultCurrentDefault()
        {
            var defaultItems = _loggerProviderInfos.Where(x => x.Value.IsDefault).ToList();
            defaultItems
                .ForEach(item =>
                {
                    _loggerProviderInfos[item.Key] = new LoggerProviderInfo(item.Value.Name, item.Value.LoggerProvider, false);
                });
        }

        private class LoggerProviderInfo
        {
            public readonly bool IsDefault;
            public readonly ILoggerProvider LoggerProvider;
            public readonly string Name;
            public LoggerProviderInfo(string name, ILoggerProvider loggerProvider, bool isDefault)
            {
                Name = name;
                LoggerProvider = loggerProvider;
                IsDefault = isDefault;
            }
        }
    }
}
