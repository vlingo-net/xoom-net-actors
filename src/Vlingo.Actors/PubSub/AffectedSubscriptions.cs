// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Actors.PubSub
{
    public class AffectedSubscriptions
    {
        private readonly IDictionary<Topic, ISubscriber> registry;

        public AffectedSubscriptions()
        {
            registry = new Dictionary<Topic, ISubscriber>();
        }

        public virtual void Add(Topic topic, ISubscriber subscriber) => registry[topic] = subscriber;

        public virtual bool HasAny => registry.Count > 0;
    }
}
