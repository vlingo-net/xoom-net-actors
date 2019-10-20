// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.Plugin;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Actors.Plugin.Supervision;

namespace Vlingo.Actors
{
    public class Configuration
    {
        private readonly List<IPlugin> plugins;
        private readonly IDictionary<string, IPluginConfiguration> configurationOverrides;
        private readonly bool mergeProperties;
        private readonly Properties? properties;

        public static Configuration Define() => new Configuration();

        public static Configuration DefineAlongWith(Properties properties)
            => new Configuration(properties, true);

        public static Configuration DefineWith(Properties properties)
            => new Configuration(properties, false);

        public IReadOnlyList<IPlugin> AllPlugins() => plugins.AsReadOnly();

        private void AddConfigurationOverride(IPluginConfiguration configuration)
        {
            configurationOverrides[configuration.GetType().Name] = configuration;
        }

        public Configuration With(CommonSupervisorsPluginConfiguration configuration)
        {
            CommonSupervisorsPluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }

        public CommonSupervisorsPluginConfiguration? CommonSupervisorsPluginConfiguration { get; private set; }

        public Configuration With(ConcurrentQueueMailboxPluginConfiguration configuration)
        {
            ConcurrentQueueMailboxPluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public ConcurrentQueueMailboxPluginConfiguration? ConcurrentQueueMailboxPluginConfiguration { get; private set; }

        public Configuration With(DefaultSupervisorOverridePluginConfiguration configuration)
        {
            DefaultSupervisorOverridePluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public DefaultSupervisorOverridePluginConfiguration? DefaultSupervisorOverridePluginConfiguration { get; private set; }

        public Configuration With(ConsoleLoggerPluginConfiguration configuration)
        {
            ConsoleLoggerPluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public ConsoleLoggerPluginConfiguration? ConsoleLoggerPluginConfiguration { get; private set; }

        public Configuration With(ManyToOneConcurrentArrayQueuePluginConfiguration configuration)
        {
            ManyToOneConcurrentArrayQueuePluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public ManyToOneConcurrentArrayQueuePluginConfiguration? ManyToOneConcurrentArrayQueuePluginConfiguration { get; private set; }

        public Configuration With(PooledCompletesPluginConfiguration configuration)
        {
            PooledCompletesPluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public PooledCompletesPluginConfiguration? PooledCompletesPluginConfiguration { get; private set; }

        public Configuration With(SharedRingBufferMailboxPluginConfiguration configuration)
        {
            SharedRingBufferMailboxPluginConfiguration = configuration;
            AddConfigurationOverride(configuration);
            return this;
        }
        public SharedRingBufferMailboxPluginConfiguration? SharedRingBufferMailboxPluginConfiguration { get; private set; }

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

        public void StartPlugins(World world, int pass)
        {
            Load(pass);

            foreach (var plugin in plugins)
            {
                if (plugin.Pass == pass)
                {
                    plugin.Start(world);
                }
            }
        }

        public void Load(int pass)
        {
            if (pass == 0)
            {
                if (properties != null)
                {
                    if (mergeProperties)
                    {
                        var plugins = LoadPlugins(false);
                        plugins.AddRange(LoadPropertiesPlugins(properties, plugins));
                    }
                    else
                    {
                        plugins.AddRange(LoadPropertiesPlugins(properties, new List<IPlugin>(0)));
                    }
                }
                else
                {
                    plugins.AddRange(LoadPlugins(true));
                }
            }
        }

        private IPluginConfiguration? OverrideConfiguration(IPlugin plugin)
        {
            if(configurationOverrides.TryGetValue(plugin.Configuration.GetType().Name, out var val))
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
            configurationOverrides = new Dictionary<string, IPluginConfiguration>();
            plugins = new List<IPlugin>();
            this.properties = properties;
            mergeProperties = includeBaseLoad;

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

            var plugins = new List<IPlugin>();
            foreach (var pluginClass in pluginClasses)
            {
                try
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginClass);
                    var pc = OverrideConfiguration(plugin);
                    var reallyBuild = pc == null ? build : false;
                    var configuredPlugin = plugin.With(pc);

                    if (reallyBuild)
                    {
                        configuredPlugin.Configuration.Build(this);
                    }

                    plugins.Add(configuredPlugin);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException($"Cannot load plugin class: {pluginClass.FullName}");
                }
            }

            return plugins;
        }
    }
}
