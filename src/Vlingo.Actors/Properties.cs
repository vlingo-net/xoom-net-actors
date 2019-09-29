// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vlingo.Actors
{
    // TODO: implement using IAppSettingsProvider later
    public sealed class Properties
    {
        private static Func<Properties> Factory = () =>
        {
            var props = new Properties();
            props.Load(new FileInfo("vlingo-actors.json"));
            return props;
        };

        private static Lazy<Properties> SingleInstance => new Lazy<Properties>(Factory, true);

        public static Properties Instance => SingleInstance.Value;

        private readonly IDictionary<string, string> dictionary;
        public Properties()
        {
            dictionary = new Dictionary<string, string>();
        }

        public ICollection<string> Keys => dictionary.Keys;

        public bool IsEmpty => dictionary.Count == 0;

        public string GetProperty(string key) => GetProperty(key, null);

        public string GetProperty(string key, string defaultValue)
        {
            if(dictionary.TryGetValue(key, out string value))
            {
                return value;
            }

            return defaultValue;
        }

        public void SetProperty(string key, string value)
        {
            dictionary[key] = value;
        }

        public void Load(FileInfo configFile)
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(configFile.Name, optional: false, reloadOnChange: true)
              .Build();

            var configurations = config.AsEnumerable().Where(c => c.Value != null);

            foreach(var configuration in configurations)
            {
                var k = configuration.Key.Replace(":",".");
                var v = configuration.Value;
                SetProperty(k, v);
            }
        }
    }
}
