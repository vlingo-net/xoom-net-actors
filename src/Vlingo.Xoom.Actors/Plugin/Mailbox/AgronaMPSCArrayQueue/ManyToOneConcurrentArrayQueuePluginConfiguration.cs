// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueuePluginConfiguration : IPluginConfiguration
    {
        private ManyToOneConcurrentArrayQueuePluginConfiguration()
        {
            Name = "arrayQueueMailbox";
        }

        public static ManyToOneConcurrentArrayQueuePluginConfiguration Define()
            => new ManyToOneConcurrentArrayQueuePluginConfiguration();

        public ManyToOneConcurrentArrayQueuePluginConfiguration WithDefaultMailbox()
        {
            IsDefaultMailbox = true;
            return this;
        }

        public ManyToOneConcurrentArrayQueuePluginConfiguration WithDispatcherThrottlingCount(int dispatcherThrottlingCount)
        {
            DispatcherThrottlingCount = dispatcherThrottlingCount;
            return this;
        }

        public ManyToOneConcurrentArrayQueuePluginConfiguration WithFixedBackoff(int fixedBackoff)
        {
            FixedBackoff = fixedBackoff;
            return this;
        }
        
        public ManyToOneConcurrentArrayQueuePluginConfiguration WithNotifyOnSend(bool notifyOnSend)
        {
            NotifyOnSend = notifyOnSend;
            return this;
        }

        public ManyToOneConcurrentArrayQueuePluginConfiguration WithRingSize(int ringSize)
        {
            RingSize = ringSize;
            return this;
        }

        public ManyToOneConcurrentArrayQueuePluginConfiguration WithSendRetires(int sendRetires)
        {
            SendRetires = sendRetires;
            return this;
        }

        public bool IsDefaultMailbox { get; private set; }

        public int DispatcherThrottlingCount { get; private set; }

        public int FixedBackoff { get; private set; }
        
        public bool NotifyOnSend { get; private set; }

        public int RingSize { get; private set; }

        public int SendRetires { get; private set; }

        public string Name { get; private set; }

        public void Build(Configuration configuration)
        {
            configuration.With(
                WithRingSize(65535)
                .WithDispatcherThrottlingCount(1)
                .WithFixedBackoff(2)
                .WithNotifyOnSend(false)
                .WithSendRetires(10)
            );
        }

        public void BuildWith(Configuration configuration, PluginProperties properties)
        {
            Name = properties.Name;
            IsDefaultMailbox = properties.GetBoolean("defaultMailbox", false);
            DispatcherThrottlingCount = properties.GetInteger("dispatcherThrottlingCount", 1);
            FixedBackoff = properties.GetInteger("fixedBackoff", 2);
            NotifyOnSend = properties.GetBoolean("notifyOnSend", false);
            RingSize = properties.GetInteger("size", 65535);
            SendRetires = properties.GetInteger("sendRetires", 10);
            configuration.With(this);
        }
    }
}
