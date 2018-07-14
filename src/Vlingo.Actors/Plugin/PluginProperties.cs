using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vlingo.Actors.Plugin
{
    public class PluginProperties
    {
        private readonly string name;
        private Properties properties;

        public PluginProperties(string name, Properties properties)
        {
            this.name = name;
            this.properties = properties;
        }

        public Boolean GetBoolean(string key, bool defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return bool.Parse(value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return float.Parse(value);
        }

        public int GetInteger(string key, int defaultValue)
        {
            var value = GetString(key, defaultValue.ToString());
            return int.Parse(value);
        }

        public string GetString(string key, string defaultValue) => properties.GetProperty(Key(key), defaultValue);

        private string Key(string key) => "plugin." + name + "." + key;
    }


    // TODO: implement using IAppSettingsProvider later
    public sealed class Properties
    {
        private readonly IDictionary<string, string> dictionary;
        public Properties()
        {
            dictionary = new Dictionary<string, string>();
        }

        public ICollection<string> Keys => dictionary.Keys;

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
            foreach(var line in File.ReadAllLines(configFile.FullName))
            {
                var items = line.Split('=');
                var key = items[0];
                var val = string.Join('=', items.Skip(1));
                SetProperty(key, val);
            }
        }
    }
}
