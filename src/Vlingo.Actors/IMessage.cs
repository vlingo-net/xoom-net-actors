// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public interface IMessage
    {
        Actor Actor { get; }
        void Deliver();
        Type Protocol { get; }
        string Representation { get; }
        bool IsStowed { get; }
        void Set<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation);
    }
}
