// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Eviction;
using Vlingo.Actors.Plugin.Logging.Console;
using Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer;
using Vlingo.Actors.Plugin.Supervision;
using Vlingo.Actors.Tests.Supervision;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ConfigurationTest
    {
        [Fact]
        public void TestThatConfigurationConfirgures()
        {
            var configuration = Configuration
                .Define()
                .With(PooledCompletesPluginConfiguration
                        .Define()
                        .WithMailbox("queueMailbox")
                        .WithPoolSize(10))
                .With(SharedRingBufferMailboxPluginConfiguration
                        .Define()
                        .WithRingSize(65535)
                        .WithFixedBackoff(2)
                        .WithNotifyOnSend(true)
                        .WithDispatcherThrottlingCount(10))
                .With(ManyToOneConcurrentArrayQueuePluginConfiguration
                        .Define()
                        .WithRingSize(65535)
                        .WithFixedBackoff(2)
                        .WithNotifyOnSend(true)
                        .WithDispatcherThrottlingCount(10)
                        .WithSendRetires(10))
                .With(ConcurrentQueueMailboxPluginConfiguration
                        .Define()
                        .WithDefaultMailbox()
                        .WithNumberOfDispatchersFactor(1.5f)
                        .WithNumberOfDispatchers(0)
                        .WithDispatcherThrottlingCount(10))
                .With(ConsoleLoggerPluginConfiguration
                        .Define()
                        .WithDefaultLogger()
                        .WithName("vlingo-net/actors(test)"))
                .With(CommonSupervisorsPluginConfiguration
                        .Define()
                        .WithSupervisor("default", "pingSupervisor", typeof(IPing), typeof(PingSupervisorActor))
                        .WithSupervisor("default", "pongSupervisor", typeof(IPong), typeof(PongSupervisorActor)))
                .With(DefaultSupervisorOverridePluginConfiguration
                        .Define()
                        .WithSupervisor("default", "overrideSupervisor", typeof(DefaultSupervisorOverride)))
                .With(DirectoryEvictionConfiguration
                    .Define()
                    .WithFillRatioHigh(0.75F)
                    .WithLruThresholdMillis(10000))
                .UsingMainProxyGeneratedClassesPath("target/classes/")
                .UsingMainProxyGeneratedSourcesPath("target/generated-sources/")
                .UsingTestProxyGeneratedClassesPath("target/test-classes/")
                .UsingTestProxyGeneratedSourcesPath("target/generated-test-sources/");

            Assert.NotNull(configuration);
            Assert.NotNull(configuration.PooledCompletesPluginConfiguration);
            Assert.Equal("queueMailbox", configuration.PooledCompletesPluginConfiguration.Mailbox);
            Assert.Equal(10, configuration.PooledCompletesPluginConfiguration.PoolSize);

            Assert.NotNull(configuration.SharedRingBufferMailboxPluginConfiguration);
            Assert.False(configuration.SharedRingBufferMailboxPluginConfiguration.IsDefaultMailbox);
            Assert.Equal(65535, configuration.SharedRingBufferMailboxPluginConfiguration.RingSize);
            Assert.Equal(2, configuration.SharedRingBufferMailboxPluginConfiguration.FixedBackoff);
            Assert.True(configuration.SharedRingBufferMailboxPluginConfiguration.NotifyOnSend);
            Assert.Equal(10, configuration.SharedRingBufferMailboxPluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ManyToOneConcurrentArrayQueuePluginConfiguration);
            Assert.False(configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.IsDefaultMailbox);
            Assert.Equal(65535, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.RingSize);
            Assert.Equal(2, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.FixedBackoff);
            Assert.True(configuration.SharedRingBufferMailboxPluginConfiguration.NotifyOnSend);
            Assert.Equal(10, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ConcurrentQueueMailboxPluginConfiguration);
            Assert.True(configuration.ConcurrentQueueMailboxPluginConfiguration.IsDefaultMailbox);
            Assert.Equal(1.5f, configuration.ConcurrentQueueMailboxPluginConfiguration.NumberOfDispatchersFactor, 0);
            Assert.Equal(10, configuration.ConcurrentQueueMailboxPluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ConsoleLoggerPluginConfiguration);
            Assert.True(configuration.ConsoleLoggerPluginConfiguration.IsDefaultLogger);
            Assert.Equal("vlingo-net/actors(test)", configuration.ConsoleLoggerPluginConfiguration.Name);

            Assert.NotNull(configuration.CommonSupervisorsPluginConfiguration);
            Assert.Equal(2, configuration.CommonSupervisorsPluginConfiguration.Count);
            Assert.Equal("default", configuration.CommonSupervisorsPluginConfiguration.StageName(0));
            Assert.Equal("pingSupervisor", configuration.CommonSupervisorsPluginConfiguration.SupervisorName(0));
            Assert.Equal(typeof(IPing), configuration.CommonSupervisorsPluginConfiguration.SupervisedProtocol(0));
            Assert.Equal(typeof(PingSupervisorActor), configuration.CommonSupervisorsPluginConfiguration.SupervisorClass(0));
            Assert.Equal("default", configuration.CommonSupervisorsPluginConfiguration.StageName(1));
            Assert.Equal("pongSupervisor", configuration.CommonSupervisorsPluginConfiguration.SupervisorName(1));
            Assert.Equal(typeof(IPong), configuration.CommonSupervisorsPluginConfiguration.SupervisedProtocol(1));
            Assert.Equal(typeof(PongSupervisorActor), configuration.CommonSupervisorsPluginConfiguration.SupervisorClass(1));

            Assert.NotNull(configuration.DefaultSupervisorOverridePluginConfiguration);
            Assert.Equal(1, configuration.DefaultSupervisorOverridePluginConfiguration.Count);
            Assert.Equal("default", configuration.DefaultSupervisorOverridePluginConfiguration.StageName(0));
            Assert.Equal("overrideSupervisor", configuration.DefaultSupervisorOverridePluginConfiguration.Name);
            Assert.Equal(typeof(DefaultSupervisorOverride), configuration.DefaultSupervisorOverridePluginConfiguration.SupervisorClass(0));

            Assert.Equal("directoryEviction", configuration.DirectoryEvictionConfiguration.Name);
            Assert.Equal(10000, configuration.DirectoryEvictionConfiguration.LruThresholdMillis);
            Assert.Equal(0.75F, configuration.DirectoryEvictionConfiguration.FillRatioHigh, 0);
            
            Assert.Equal("target/classes/", configuration.MainProxyGeneratedClassesPath);
            Assert.Equal("target/generated-sources/", configuration.MainProxyGeneratedSourcesPath);
            Assert.Equal("target/test-classes/", configuration.TestProxyGeneratedClassesPath);
            Assert.Equal("target/generated-test-sources/", configuration.TestProxyGeneratedSourcesPath);
        }

        [Fact]
        public void TestThatConfigurationDefaults()
        {
            var configuration = Configuration.Define();
            configuration.Load(0);

            Assert.NotNull(configuration);
            Assert.NotNull(configuration.PooledCompletesPluginConfiguration);
            Assert.Equal("queueMailbox", configuration.PooledCompletesPluginConfiguration.Mailbox);
            Assert.Equal(10, configuration.PooledCompletesPluginConfiguration.PoolSize);

            Assert.NotNull(configuration.SharedRingBufferMailboxPluginConfiguration);
            Assert.False(configuration.SharedRingBufferMailboxPluginConfiguration.IsDefaultMailbox);
            Assert.Equal(65535, configuration.SharedRingBufferMailboxPluginConfiguration.RingSize);
            Assert.Equal(2, configuration.SharedRingBufferMailboxPluginConfiguration.FixedBackoff);
            Assert.False(configuration.SharedRingBufferMailboxPluginConfiguration.NotifyOnSend);
            Assert.Equal(10, configuration.SharedRingBufferMailboxPluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ManyToOneConcurrentArrayQueuePluginConfiguration);
            Assert.False(configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.IsDefaultMailbox);
            Assert.Equal(65535, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.RingSize);
            Assert.Equal(2, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.FixedBackoff);
            Assert.False(configuration.SharedRingBufferMailboxPluginConfiguration.NotifyOnSend);
            Assert.Equal(1, configuration.ManyToOneConcurrentArrayQueuePluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ConcurrentQueueMailboxPluginConfiguration);
            Assert.True(configuration.ConcurrentQueueMailboxPluginConfiguration.IsDefaultMailbox);
            Assert.Equal(1.5f, configuration.ConcurrentQueueMailboxPluginConfiguration.NumberOfDispatchersFactor, 0);
            Assert.Equal(1, configuration.ConcurrentQueueMailboxPluginConfiguration.DispatcherThrottlingCount);

            Assert.NotNull(configuration.ConsoleLoggerPluginConfiguration);
            Assert.True(configuration.ConsoleLoggerPluginConfiguration.IsDefaultLogger);
            Assert.Equal("vlingo-net/actors", configuration.ConsoleLoggerPluginConfiguration.Name);

            Assert.NotNull(configuration.DefaultSupervisorOverridePluginConfiguration);
            Assert.Equal(1, configuration.DefaultSupervisorOverridePluginConfiguration.Count);
            Assert.Equal("default", configuration.DefaultSupervisorOverridePluginConfiguration.StageName(0));
            Assert.Equal("overrideSupervisor", configuration.DefaultSupervisorOverridePluginConfiguration.SupervisorName(0));
            Assert.Equal(typeof(DefaultSupervisorOverride), configuration.DefaultSupervisorOverridePluginConfiguration.SupervisorClass(0));

            Assert.Equal("target/classes/", configuration.MainProxyGeneratedClassesPath);
            Assert.Equal("target/generated-sources/", configuration.MainProxyGeneratedSourcesPath);
            Assert.Equal("target/test-classes/", configuration.TestProxyGeneratedClassesPath);
            Assert.Equal("target/generated-test-sources/", configuration.TestProxyGeneratedSourcesPath);
        }
    }
}
