﻿// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.Plugin;

public class PluginLoader
{
    private const string PluginNamePrefix = "plugin.name.";

    private readonly IDictionary<string, IPlugin> _plugins;

    public PluginLoader() => _plugins = new Dictionary<string, IPlugin>();

    public IEnumerable<IPlugin> LoadEnabledPlugins(Configuration configuration, Properties properties) => 
        LoadEnabledPlugins(configuration, properties, new StaticPluginTypeLoader());

    public IEnumerable<IPlugin> LoadEnabledPlugins(Configuration configuration, Properties properties, IPluginTypeLoader pluginTypeLoader)
    {
        if (!properties.IsEmpty)
        {
            foreach(var enabledPlugin in FindEnabledPlugins(properties))
            {
                LoadPlugin(configuration, properties, enabledPlugin, pluginTypeLoader);
            }
        }

        return _plugins.Values;
    }

    private ISet<string> FindEnabledPlugins(Properties properties)
    {
        var enabledPlugins = new HashSet<string>();
        foreach(var key in properties.Keys)
        {
            if (key.StartsWith(PluginNamePrefix))
            {
                if (bool.TryParse(properties.GetProperty(key), out bool _))
                {
                    enabledPlugins.Add(key);
                }
            }
        }

        return enabledPlugins;
    }

    private void LoadPlugin(Configuration configuration, Properties properties, string enabledPlugin, IPluginTypeLoader pluginTypeLoader)
    {
        var pluginName = enabledPlugin.Substring(PluginNamePrefix.Length);
        var classNameKey = $"plugin.{pluginName}.classname";
        var className = properties.GetProperty(classNameKey) ?? throw new ArgumentException("properties.GetProperty(classNameKey)");
        var pluginUniqueName = $"{pluginName}:{className}";
            
        try
        {
            if (!_plugins.TryGetValue(pluginUniqueName, out _))
            {
                if (!_plugins.TryGetValue(className, out var plugin))
                {
                    var pluginClass = pluginTypeLoader.LoadType(className);
                    plugin = (IPlugin)Activator.CreateInstance(pluginClass!, pluginName)!;
                    _plugins[pluginUniqueName] = plugin;
                }   
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw new ArgumentException($"Could not load plugin {className}");
        }
    }
}