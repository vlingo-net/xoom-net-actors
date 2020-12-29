// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueuePlugin : AbstractPlugin, IMailboxProvider
    {
        private readonly ManyToOneConcurrentArrayQueuePluginConfiguration _configuration;
        private readonly ConcurrentDictionary<int, ManyToOneConcurrentArrayQueueDispatcher> _dispatchers;

        public ManyToOneConcurrentArrayQueuePlugin(string? name = null)
        {
            _configuration = ManyToOneConcurrentArrayQueuePluginConfiguration.Define();
            _dispatchers = new ConcurrentDictionary<int, ManyToOneConcurrentArrayQueueDispatcher>(16, 1);
        }
        private ManyToOneConcurrentArrayQueuePlugin(IPluginConfiguration configuration)
        {
            _configuration = (ManyToOneConcurrentArrayQueuePluginConfiguration)configuration;
            _dispatchers = new ConcurrentDictionary<int, ManyToOneConcurrentArrayQueueDispatcher>(16, 1);
        }

        public override void Close() => _dispatchers.Values.ToList().ForEach(x => x.Close());

        public override string Name => _configuration.Name;

        public override int Pass => 1;

        public override IPluginConfiguration Configuration => _configuration;

        public override void Start(IRegistrar registrar)
            => registrar.Register(_configuration.Name, _configuration.IsDefaultMailbox, this);

        public IMailbox ProvideMailboxFor(int? hashCode) => ProvideMailboxFor(hashCode, null);

        public IMailbox ProvideMailboxFor(int? hashCode, IDispatcher? dispatcher)
        {
            ManyToOneConcurrentArrayQueueDispatcher? maybeDispatcher;

            if (!hashCode.HasValue)
            {
                throw new ArgumentNullException(nameof(hashCode),"Cannot provide mailbox because the hashCode is null.");
            }
            
            if (dispatcher != null)
            {
                maybeDispatcher = (ManyToOneConcurrentArrayQueueDispatcher)dispatcher;
            }
            else
            {
                _dispatchers.TryGetValue(hashCode.Value, out maybeDispatcher);
            }

            if (maybeDispatcher == null)
            {
                var newDispatcher = new ManyToOneConcurrentArrayQueueDispatcher(
                    _configuration.RingSize,
                    _configuration.FixedBackoff,
                    _configuration.NotifyOnSend,
                    _configuration.DispatcherThrottlingCount,
                    _configuration.SendRetires);

                var otherDispatcher = _dispatchers.GetOrAdd(hashCode.Value, newDispatcher);
                
                otherDispatcher.Start();
                return otherDispatcher.Mailbox;
            }
            return maybeDispatcher.Mailbox;
        }

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new ManyToOneConcurrentArrayQueuePlugin(overrideConfiguration);
    }
}
