// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxPlugin : AbstractPlugin, IMailboxProvider
    {
        private readonly ConcurrentQueueMailboxPluginConfiguration _configuration;
        private IDispatcher? _executorDispatcher;

        public ConcurrentQueueMailboxPlugin(string name) => _configuration = ConcurrentQueueMailboxPluginConfiguration.Define(name);

        private ConcurrentQueueMailboxPlugin(IPluginConfiguration configuration) => _configuration = (ConcurrentQueueMailboxPluginConfiguration)configuration;

        public override string Name => _configuration.Name;

        public override int Pass => 1;

        public override IPluginConfiguration Configuration => _configuration;

        public override void Start(IRegistrar registrar)
        {
            _executorDispatcher = new ExecutorDispatcher(
                System.Environment.ProcessorCount,
                _configuration.NumberOfDispatchers,
                _configuration.NumberOfDispatchersFactor);
            registrar.Register(_configuration.Name, _configuration.IsDefaultMailbox, this);
        }

        public IMailbox ProvideMailboxFor(int? hashCode) 
            => new ConcurrentQueueMailbox(_executorDispatcher!, _configuration.DispatcherThrottlingCount);

        public IMailbox ProvideMailboxFor(int? hashCode, IDispatcher dispatcher)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            return new ConcurrentQueueMailbox(dispatcher, _configuration.DispatcherThrottlingCount);
        }

        public override void Close() => _executorDispatcher!.Close();

        public override IPlugin With(IPluginConfiguration? overrideConfiguration)
            => overrideConfiguration == null ? this : new ConcurrentQueueMailboxPlugin(overrideConfiguration);
    }
}
