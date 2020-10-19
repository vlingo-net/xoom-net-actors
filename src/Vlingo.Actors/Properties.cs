// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public sealed class Properties: ConfigurationProperties
    {
        private static Func<Properties> Factory = () =>
        {
            var props = new Properties();
            props.Load(new FileInfo("vlingo-actors.json"));
            return props;
        };

        private static Lazy<Properties> SingleInstance => new Lazy<Properties>(Factory, true);

        public static Properties Instance => SingleInstance.Value;

        public static long GetLong(string key, long defaultValue) => Get(key, long.Parse!, defaultValue);
        
        public static float GetFloat(string key, float defaultValue) => Get(key, float.Parse!, defaultValue);

        private static T Get<T>(string key, Func<string?, T> parse, T defaultValue)
        {
            var property = Instance.GetProperty(key);
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    return parse(property);
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }
    }
}
