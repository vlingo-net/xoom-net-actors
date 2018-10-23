// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
        private ConcurrentQueueMailboxPluginConfiguration concurrentQueueMailboxPluginConfiguration;
        private CommonSupervisorsPluginConfiguration commonSupervisorsPluginConfiguration;
        private DefaultSupervisorOverridePluginConfiguration defaultSupervisorOverridePluginConfiguration;
        private ConsoleLoggerPluginConfiguration jdkLoggerPluginConfiguration;
        private PooledCompletesPluginConfiguration pooledCompletesPluginConfiguration;
        private ManyToOneConcurrentArrayQueuePluginConfiguration manyToOneConcurrentArrayQueuePluginConfiguration;
        private SharedRingBufferMailboxPluginConfiguration sharedRingBufferMailboxPluginConfiguration;
        private string mainProxyGeneratedClassesPath;
        private string mainProxyGeneratedSourcesPath;
        private string testProxyGeneratedClassesPath;
        private string testProxyGeneratedSourcesPath;
        private List<IPlugin> plugins;

        public static Configuration Define() => new Configuration();

        public static Configuration DefineAlongWith(Properties properties)
            => new Configuration(properties, true);

        public static Configuration DefineWith(Properties properties)
            => new Configuration(properties, false);

        public IReadOnlyList<IPlugin> AllPlugins() => plugins.AsReadOnly();

        public Configuration With(CommonSupervisorsPluginConfiguration configuration)
        {
            CommonSupervisorsPluginConfiguration = configuration;
            return this;
        }
        public CommonSupervisorsPluginConfiguration CommonSupervisorsPluginConfiguration { get; private set; }

        public Configuration With(ConcurrentQueueMailboxPluginConfiguration configuration)
        {
            ConcurrentQueueMailboxPluginConfiguration = configuration;
            return this;
        }
        public ConcurrentQueueMailboxPluginConfiguration ConcurrentQueueMailboxPluginConfiguration { get; private set; }

        public Configuration With(DefaultSupervisorOverridePluginConfiguration configuration)
        {
            DefaultSupervisorOverridePluginConfiguration = configuration;
            return this;
        }
        public DefaultSupervisorOverridePluginConfiguration DefaultSupervisorOverridePluginConfiguration { get; private set; }

        public Configuration With(ConsoleLoggerPluginConfiguration configuration)
        {
            ConsoleLoggerPluginConfiguration = configuration;
            return this;
        }
        public ConsoleLoggerPluginConfiguration ConsoleLoggerPluginConfiguration { get; private set; }

        public Configuration With(ManyToOneConcurrentArrayQueuePluginConfiguration configuration)
        {
            ManyToOneConcurrentArrayQueuePluginConfiguration = configuration;
            return this;
        }
        public ManyToOneConcurrentArrayQueuePluginConfiguration ManyToOneConcurrentArrayQueuePluginConfiguration { get; private set; }

        public Configuration With(PooledCompletesPluginConfiguration configuration)
        {
            PooledCompletesPluginConfiguration = configuration;
            return this;
        }
        public PooledCompletesPluginConfiguration PooledCompletesPluginConfiguration { get; private set; }

        public Configuration With(SharedRingBufferMailboxPluginConfiguration configuration)
        {
            SharedRingBufferMailboxPluginConfiguration = configuration;
            return this;
        }
        public SharedRingBufferMailboxPluginConfiguration SharedRingBufferMailboxPluginConfiguration { get; private set; }

        public Configuration UsingMainProxyGeneratedClassesPath(string path)
        {
            MainProxyGeneratedClassesPath = path;
            return this;
        }
        public string MainProxyGeneratedClassesPath { get; private set; }

        public Configuration UsingMainProxyGeneratedSourcesPath(string path)
        {
            MainProxyGeneratedSourcesPath = path;
            return this;
        }
        public string MainProxyGeneratedSourcesPath { get; private set; }

        public Configuration UsingTestProxyGeneratedClassesPath(string path)
        {
            TestProxyGeneratedClassesPath = path;
            return this;
        }
        public string TestProxyGeneratedClassesPath { get; private set; }

        public Configuration UsingTestProxyGeneratedSourcesPath(string path)
        {
            TestProxyGeneratedSourcesPath = path;
            return this;
        }
        public string TestProxyGeneratedSourcesPath { get; private set; }

        public void StartPlugins(World world, int pass)
        {
            foreach (var plugin in plugins)
            {
                if (plugin.Pass == pass)
                {
                    plugin.Start(world);
                }
            }
        }
        private Configuration()
        {
            plugins = LoadPlugins(true);

            UsingMainProxyGeneratedClassesPath("target/classes/")
            .UsingMainProxyGeneratedSourcesPath("target/generated-sources/")
            .UsingTestProxyGeneratedClassesPath("target/test-classes/")
            .UsingTestProxyGeneratedSourcesPath("target/generated-test-sources/");
        }

        private Configuration(Properties properties, bool includeBaseLoad)
        {
            if (includeBaseLoad)
            {
                var plugins = LoadPlugins(false);
                this.plugins = LoadPropertiesPlugins(properties, plugins);
            }
            else
            {
                plugins = LoadPropertiesPlugins(properties, new List<IPlugin>());
            }
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
                    if (build)
                    {
                        plugin.Configuration.Build(this);
                    }
                    plugins.Add(plugin);
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
