// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxPluginConfiguration : IPluginConfiguration
    {
        private ConcurrentQueueMailboxPluginConfiguration(string? name = null) => Name = name ?? "queueMailbox";

        public static ConcurrentQueueMailboxPluginConfiguration Define(string? name = null) => new ConcurrentQueueMailboxPluginConfiguration(name);

        public ConcurrentQueueMailboxPluginConfiguration WithDefaultMailbox()
        {
            IsDefaultMailbox = true;
            return this;
        }

        public bool IsDefaultMailbox { get; private set; }

        public ConcurrentQueueMailboxPluginConfiguration WithDispatcherThrottlingCount(int dispatcherThrottlingCount)
        {
            DispatcherThrottlingCount = dispatcherThrottlingCount;
            return this;
        }

        public int DispatcherThrottlingCount { get; private set; }

        public ConcurrentQueueMailboxPluginConfiguration WithNumberOfDispatchersFactor(float numberOfDispatchersFactor)
        {
            NumberOfDispatchersFactor = numberOfDispatchersFactor;
            return this;
        }
        
        public ConcurrentQueueMailboxPluginConfiguration WithNumberOfDispatchers(int numberOfDispatchers)
        {
            NumberOfDispatchers = numberOfDispatchers;
            return this;
        }

        public float NumberOfDispatchersFactor { get; private set; }
        
        public int NumberOfDispatchers { get; private set; }

        public string Name { get; private set; }

        public void Build(Configuration configuration)
        {
            configuration.With(
                WithDefaultMailbox()
                .WithNumberOfDispatchersFactor(1.5f)
                .WithDispatcherThrottlingCount(1));
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            Name = properties.Name;
            IsDefaultMailbox = properties.GetBoolean("defaultMailbox", true);
            DispatcherThrottlingCount = properties.GetInteger("dispatcherThrottlingCount", 1);
            NumberOfDispatchersFactor = properties.GetFloat("numberOfDispatchersFactor", 1.5f);
        }
    }
}
