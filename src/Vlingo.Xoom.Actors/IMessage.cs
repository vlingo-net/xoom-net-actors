// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq.Expressions;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    public interface IMessage
    {
        Actor Actor { get; }
        ICompletes? Completes { get; }
        void Deliver();
        Type Protocol { get; }
        string Representation { get; }
        bool IsStowed { get; }
        LambdaExpression? SerializableConsumer { get; }
        void Set<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation);
        void Set(Actor actor, Type protocol, LambdaExpression? consumer, ICompletes? completes, string representation);
    }
}
