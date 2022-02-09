// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Globalization;

namespace Vlingo.Xoom.Actors.Plugin
{
    public class PluginProperties
    {
        private readonly Properties _properties;

        public PluginProperties(string name, Properties properties)
        {
            Name = name;
            _properties = properties;
        }

        public string Name { get; }

        public bool GetBoolean(string key, bool defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return value == null ? defaultValue : bool.Parse(value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            var value = GetString(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            return value == null ? defaultValue : float.Parse(value, CultureInfo.InvariantCulture);
        }

        public int GetInteger(string key, int defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return value == null ? defaultValue : int.Parse(value);
        }
        
        public long GetLong(string key, long defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return value == null ? defaultValue : long.Parse(value);
        }

        public string? GetString(string key, string? defaultValue) => _properties.GetProperty(Key(key), defaultValue);

        private string Key(string key) => $"plugin.{Name}.{key}";
    }
}
