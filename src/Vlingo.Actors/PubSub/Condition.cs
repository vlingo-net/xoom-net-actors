// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace Vlingo.Actors.PubSub
{
    internal delegate bool Condition(KeyValuePair<Topic, ISet<ISubscriber>> subscription, Topic topic, ISubscriber subscriber);
    
    internal static class ConditionExtension
    {
        public static bool Should(
            this Condition condition, 
            KeyValuePair<Topic, ISet<ISubscriber>> subscription, 
            Topic topic, 
            ISubscriber subscriber)
            => condition.Invoke(subscription, topic, subscriber);
    }
}
