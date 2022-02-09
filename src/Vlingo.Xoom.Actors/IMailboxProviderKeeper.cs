// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    public interface IMailboxProviderKeeper
    {
        IMailbox AssignMailbox(string name, int? hashCode);
        void Close();
        string FindDefault();
        void Keep(string name, bool isDefault, IMailboxProvider mailboxProvider);
        bool IsValidMailboxName(string candidateMailboxName);
    }
}
