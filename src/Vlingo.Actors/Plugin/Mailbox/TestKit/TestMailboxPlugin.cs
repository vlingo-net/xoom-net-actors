// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailboxPlugin : IPlugin, IMailboxProvider
    {
        public TestMailboxPlugin(IRegistrar registrar)
        {
            Start(registrar, TestMailbox.Name, null);
        }

        public string Name { get; private set; }

        public int Pass => 1;

        public void Close()
        {
        }

        public IMailbox ProvideMailboxFor(int hashCode) => new TestMailbox();

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher) => new TestMailbox();

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;
            registrar.Register(name, false, this);
        }
    }
}
