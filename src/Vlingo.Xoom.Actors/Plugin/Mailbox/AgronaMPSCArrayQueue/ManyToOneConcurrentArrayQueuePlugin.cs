// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;

public class ManyToOneConcurrentArrayQueuePlugin : AbstractPlugin, IMailboxProvider
{
    private readonly ManyToOneConcurrentArrayQueuePluginConfiguration _configuration;

    public ManyToOneConcurrentArrayQueuePlugin(string? name = null) => 
        _configuration = ManyToOneConcurrentArrayQueuePluginConfiguration.Define();

    internal ManyToOneConcurrentArrayQueuePlugin(IPluginConfiguration configuration) => 
        _configuration = (ManyToOneConcurrentArrayQueuePluginConfiguration)configuration;

    public override void Close()
    {
        // mailbox closes its dispatcher
    }

    public override string Name => _configuration.Name;

    public override int Pass => 1;

    public override IPluginConfiguration Configuration => _configuration;

    public override void Start(IRegistrar registrar)
        => registrar.Register(_configuration.Name, _configuration.IsDefaultMailbox, this);

    public IMailbox ProvideMailboxFor(int? hashCode)
    {
        var newDispatcher = new ManyToOneConcurrentArrayQueueDispatcher(
            _configuration.RingSize,
            _configuration.FixedBackoff,
            _configuration.NotifyOnSend,
            _configuration.DispatcherThrottlingCount,
            _configuration.SendRetires);
        
        newDispatcher.Start();
        
        return newDispatcher.Mailbox;
    }

    public IMailbox ProvideMailboxFor(int? hashCode, IDispatcher? dispatcher) => 
        throw new InvalidOperationException("Does not support dispatcher reuse.");

    public override IPlugin With(IPluginConfiguration? overrideConfiguration)
        => overrideConfiguration == null ? this : new ManyToOneConcurrentArrayQueuePlugin(overrideConfiguration);
}