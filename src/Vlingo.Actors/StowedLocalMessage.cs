// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class StowedLocalMessage<T> : LocalMessage<T>
    {
        public StowedLocalMessage(Actor actor, Action<T> consumer, string representation)
            : base(actor, consumer, representation)
        {
        }

        public StowedLocalMessage(LocalMessage<T> message)
            : base(message)
        {
        }

        public override bool IsStowed => true;
    }
}
