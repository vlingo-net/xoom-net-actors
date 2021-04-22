// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Actors
{
    public class StowedLocalMessage<T> : LocalMessage<T>
    {
        public StowedLocalMessage(Actor actor, Action<T> consumer, ICompletes<object> completes, string representation)
            : base(actor, consumer, completes, representation)
        {
        }

        public StowedLocalMessage(LocalMessage<T> message)
            : base(message)
        {
        }

        public override bool IsStowed => true;
    }
}
