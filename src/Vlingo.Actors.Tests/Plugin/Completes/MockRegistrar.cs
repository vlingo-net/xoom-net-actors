// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors.Tests.Plugin.Completes
{
    public class MockRegistrar : IRegistrar
    {
        public int registerCount;

        public World World => null;

        public void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            completesEventuallyProvider.InitializeUsing(null);
            ++registerCount;
        }

        public void Register(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
        }

        public void Register(string name, bool isDefault, IMailboxProvider mailboxProvider)
        {
        }

        public void RegisterCommonSupervisor(string stageName, string name, Type supervisedProtocol, Type supervisorClass)
        {
        }

        public void RegisterCompletesEventuallyProviderKeeper(ICompletesEventuallyProviderKeeper keeper)
        {
        }

        public void RegisterDefaultSupervisor(string stageName, string name, Type supervisorClass)
        {
        }

        public void RegisterLoggerProviderKeeper(ILoggerProviderKeeper keeper)
        {
        }

        public void RegisterMailboxProviderKeeper(IMailboxProviderKeeper keeper)
        {
        }
    }
}
