// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.PubSub
{
    public class AffectedSubscriptions
    {
        private readonly IDictionary<Topic, ISubscriber> _registry;

        public AffectedSubscriptions() => _registry = new Dictionary<Topic, ISubscriber>();

        public virtual void Add(Topic topic, ISubscriber subscriber) => _registry[topic] = subscriber;

        public virtual bool HasAny => _registry.Count > 0;
    }
}
