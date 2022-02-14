// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Xoom.Actors.Plugin;
using Vlingo.Xoom.Actors.Plugin.Completes;
using Vlingo.Xoom.Actors.Plugin.Eviction;
using Vlingo.Xoom.Actors.Plugin.Logging.Console;
using Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Xoom.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Xoom.Actors.Plugin.Supervision;

namespace Vlingo.Xoom.Actors;

public class Configuration
{
    private readonly List<IPlugin> _plugins;
    private readonly List<IPlugin> _dynamicPlugins;
    private readonly IDictionary<string, IPluginConfiguration> _configurationOverrides;
    private readonly bool _mergeProperties;
    private readonly Properties? _properties;
    private IAddressFactory? _addressFactory;

    public static Configuration Define() => new Configuration();

    public static Configuration DefineAlongWith(Properties properties)
        => new Configuration(properties, true);

    public static Configuration DefineWith(Properties properties)
        => new Configuration(properties, false);

    public IReadOnlyList<IPlugin> AllPlugins() =>
        _plugins.Concat(_dynamicPlugins).ToList();
        
    public Configuration With(IAddressFactory addressFactory)
    {
        _addressFactory = addressFactory;
        return this;
    }

    public IAddressFactory AddressFactoryOr(Func<IAddressFactory> addressFactorySupplier) => _addressFactory ?? addressFactorySupplier();

    public string? GetProperty(string key) =>
        _properties == null
            ? null
            : _properties.GetProperty(key);

    public string? GetProperty(string key, string? defaultValue) =>
        _properties == null
            ? defaultValue
            : _properties.GetProperty(key, defaultValue);

    private void AddConfigurationOverride(IPluginConfiguration configuration) => _configurationOverrides[configuration.GetType().Name] = configuration;

    public Configuration With(CommonSupervisorsPluginConfiguration configuration)
    {
        CommonSupervisorsPluginConfiguration ??= configuration;
            
        AddConfigurationOverride(configuration);
        return this;
    }

    public CommonSupervisorsPluginConfiguration? CommonSupervisorsPluginConfiguration { get; private set; }

    public Configuration With(ConcurrentQueueMailboxPluginConfiguration configuration)
    {
        ConcurrentQueueMailboxPluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public ConcurrentQueueMailboxPluginConfiguration? ConcurrentQueueMailboxPluginConfiguration { get; private set; }

    public Configuration With(DefaultSupervisorOverridePluginConfiguration configuration)
    {
        DefaultSupervisorOverridePluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public DefaultSupervisorOverridePluginConfiguration? DefaultSupervisorOverridePluginConfiguration { get; private set; }

    public Configuration With(ConsoleLoggerPluginConfiguration configuration)
    {
        ConsoleLoggerPluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public ConsoleLoggerPluginConfiguration? ConsoleLoggerPluginConfiguration { get; private set; }

    public Configuration With(ManyToOneConcurrentArrayQueuePluginConfiguration configuration)
    {
        ManyToOneConcurrentArrayQueuePluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public ManyToOneConcurrentArrayQueuePluginConfiguration? ManyToOneConcurrentArrayQueuePluginConfiguration { get; private set; }

    public Configuration With(PooledCompletesPluginConfiguration configuration)
    {
        PooledCompletesPluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public PooledCompletesPluginConfiguration? PooledCompletesPluginConfiguration { get; private set; }

    public Configuration With(SharedRingBufferMailboxPluginConfiguration configuration)
    {
        SharedRingBufferMailboxPluginConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public SharedRingBufferMailboxPluginConfiguration? SharedRingBufferMailboxPluginConfiguration { get; private set; }
        
    public Configuration With(DirectoryEvictionConfiguration configuration)
    {
        DirectoryEvictionConfiguration ??= configuration;
        AddConfigurationOverride(configuration);
        return this;
    }
        
    public DirectoryEvictionConfiguration? DirectoryEvictionConfiguration { get; private set; }

    public Configuration UsingMainProxyGeneratedClassesPath(string path)
    {
        MainProxyGeneratedClassesPath = path;
        return this;
    }
        
    public string? MainProxyGeneratedClassesPath { get; private set; }

    public Configuration UsingMainProxyGeneratedSourcesPath(string path)
    {
        MainProxyGeneratedSourcesPath = path;
        return this;
    }
        
    public string? MainProxyGeneratedSourcesPath { get; private set; }

    public Configuration UsingTestProxyGeneratedClassesPath(string path)
    {
        TestProxyGeneratedClassesPath = path;
        return this;
    }
        
    public string? TestProxyGeneratedClassesPath { get; private set; }

    public Configuration UsingTestProxyGeneratedSourcesPath(string path)
    {
        TestProxyGeneratedSourcesPath = path;
        return this;
    }
        
    public string? TestProxyGeneratedSourcesPath { get; private set; }

    public void StartPlugins(IRegistrar registrar, int pass)
    {
        Load(pass);

        foreach (var plugin in _plugins)
        {
            if (plugin.Pass == pass)
            {
                plugin.Start(registrar);
            }
        }
    }

    public void Load(int pass)
    {
        if (pass == 0)
        {
            if (_properties != null)
            {
                if (_mergeProperties)
                {
                    var plugins = LoadPlugins(false);
                    plugins.AddRange(LoadPropertiesPlugins(_properties, plugins));
                }
                else
                {
                    _plugins.AddRange(LoadPropertiesPlugins(_properties, new List<IPlugin>(0)));
                }
            }
            else
            {
                _plugins.AddRange(LoadPlugins(true));
            }
        }
    }

    public void LoadAndStartDynamicPlugins(IRegistrar registrar, IPluginTypeLoader pluginClassLoader, Properties pluginsProperties)
    {
        var dynamicPlugins = new PluginLoader()
            .LoadEnabledPlugins(this, pluginsProperties, pluginClassLoader)
            .ToList();
        dynamicPlugins.ForEach(plugin => plugin
            .Configuration
            .BuildWith(this, new PluginProperties(plugin.Name, pluginsProperties)));

        dynamicPlugins.ForEach(plugin => plugin.Start(registrar));
        _dynamicPlugins.AddRange(dynamicPlugins);
    }
        
    private IPluginConfiguration? OverrideConfiguration(IPlugin plugin)
    {
        if (_configurationOverrides.TryGetValue(plugin.Configuration.GetType().Name, out var val))
        {
            return val;
        }

        return null;
    }

    private Configuration() : this(null, false)
    {
    }

    private Configuration(Properties? properties, bool includeBaseLoad)
    {
        _configurationOverrides = new Dictionary<string, IPluginConfiguration>();
        _plugins = new List<IPlugin>();
        _dynamicPlugins = new List<IPlugin>();
        _properties = properties;
        _mergeProperties = includeBaseLoad;

        UsingMainProxyGeneratedClassesPath("target/classes/")
            .UsingMainProxyGeneratedSourcesPath("target/generated-sources/")
            .UsingTestProxyGeneratedClassesPath("target/test-classes/")
            .UsingTestProxyGeneratedSourcesPath("target/generated-test-sources/");
    }

    private List<IPlugin> LoadPropertiesPlugins(Properties properties, List<IPlugin> plugins)
    {
        var unique = new HashSet<IPlugin>(plugins);
        unique.UnionWith(new PluginLoader().LoadEnabledPlugins(this, properties));
        foreach (var plugin in unique)
        {
            plugin.Configuration.BuildWith(this, new PluginProperties(plugin.Name, properties));
        }

        return new List<IPlugin>(unique);
    }

    private List<IPlugin> LoadPlugins(bool build)
    {
        var pluginClasses = new[]
        {
            typeof(PooledCompletesPlugin),
            typeof(ConsoleLoggerPlugin),
            typeof(ManyToOneConcurrentArrayQueuePlugin),
            typeof(ConcurrentQueueMailboxPlugin),
            typeof(SharedRingBufferMailboxPlugin),
            typeof(CommonSupervisorsPlugin),
            typeof(DefaultSupervisorOverridePlugin)
        };

        var loadPlugins = new List<IPlugin>();
        foreach (var pluginClass in pluginClasses)
        {
            try
            {
                var plugin = (IPlugin) Activator.CreateInstance(pluginClass, (string) null!)!;
                var pc = OverrideConfiguration(plugin);
                var reallyBuild = pc == null && build;
                var configuredPlugin = plugin.With(pc);

                if (reallyBuild)
                {
                    configuredPlugin.Configuration.Build(this);
                }

                loadPlugins.Add(configuredPlugin);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Cannot load plugin class: {pluginClass.FullName}");
            }
        }

        return loadPlugins;
    }
}