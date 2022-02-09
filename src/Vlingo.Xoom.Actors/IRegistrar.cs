// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
{
    public interface IRegistrar
    {
        void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider);
        void Register(string name, bool isDefault, ILoggerProvider loggerProvider);
        void Register(string name, bool isDefault, IMailboxProvider mailboxProvider);
        void RegisterCommonSupervisor(string stageName, string name, Type? supervisedProtocol, Type? supervisorClass);
        void RegisterDefaultSupervisor(string stageName, string name, Type? supervisorClass);
        void RegisterCompletesEventuallyProviderKeeper(ICompletesEventuallyProviderKeeper keeper);
        void RegisterLoggerProviderKeeper(ILoggerProviderKeeper keeper);
        void RegisterMailboxProviderKeeper(IMailboxProviderKeeper keeper);
        World World { get; }
    }
}
