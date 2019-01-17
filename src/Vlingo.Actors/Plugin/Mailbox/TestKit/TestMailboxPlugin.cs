// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public sealed class TestMailboxPlugin : AbstractPlugin, IMailboxProvider
    {
        public TestMailboxPlugin(IRegistrar registrar)
        {
            Start(registrar);
        }

        public override string Name => TestMailbox.Name;

        public override int Pass => 1;

        public override IPluginConfiguration Configuration => null;

        public override void Close()
        {
        }

        public IMailbox ProvideMailboxFor(int hashCode) => new TestMailbox();

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher) => new TestMailbox();

        public override void Start(IRegistrar registrar) => registrar.Register(Name, false, this);
    }
}
