// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class SharedRingBufferMailboxPlugin : AbstractPlugin, IMailboxProvider
    {
        private readonly SharedRingBufferMailboxPluginConfiguration _configuration;
        private readonly ConcurrentDictionary<int, RingBufferDispatcher> _dispatchers;

        public SharedRingBufferMailboxPlugin(string? name = null)
        {
            _configuration = SharedRingBufferMailboxPluginConfiguration.Define();
            _dispatchers = new ConcurrentDictionary<int, RingBufferDispatcher>(16, 1);
        }

        private SharedRingBufferMailboxPlugin(IPluginConfiguration configuration)
        {
            _configuration = (SharedRingBufferMailboxPluginConfiguration)configuration;
            _dispatchers = new ConcurrentDictionary<int, RingBufferDispatcher>(16, 1);
        }

        public override string Name => _configuration.Name;

        public override int Pass => 1;

        public override IPluginConfiguration Configuration => _configuration;

        public override void Close()
            => _dispatchers.Values.ToList().ForEach(x => x.Close());

        public override void Start(IRegistrar registrar)
        {
            registrar.Register(_configuration.Name, _configuration.IsDefaultMailbox, this);
        }

        public IMailbox ProvideMailboxFor(int? hashCode) => ProvideMailboxFor(hashCode, null);

        public IMailbox ProvideMailboxFor(int? hashCode, IDispatcher? dispatcher)
        {
            RingBufferDispatcher? maybeDispatcher;
            
            if (!hashCode.HasValue)
            {
                throw new ArgumentNullException(nameof(hashCode),"Cannot provide mailbox because the hashCode is null.");
            }

            if (dispatcher != null)
            {
                maybeDispatcher = (RingBufferDispatcher)dispatcher;
            }
            else
            {
                _dispatchers.TryGetValue(hashCode.Value, out maybeDispatcher);
            }

            if (maybeDispatcher == null)
            {
                var newDispatcher = new RingBufferDispatcher(
                    _configuration.RingSize,
                    _configuration.FixedBackoff,
                    _configuration.NotifyOnSend,
                    _configuration.DispatcherThrottlingCount);

                var otherDispatcher = _dispatchers.GetOrAdd(hashCode.Value, newDispatcher);

                otherDispatcher.Start();
                return otherDispatcher.Mailbox;
            }

            return maybeDispatcher.Mailbox;
        }

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new SharedRingBufferMailboxPlugin(overrideConfiguration);
    }
}
