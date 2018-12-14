// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors.PubSub
{
    public class Subscriptions
    {
        private readonly IDictionary<Topic, ISet<ISubscriber>> index;

        public Subscriptions()
        {
            index = new Dictionary<Topic, ISet<ISubscriber>>();
        }

        public virtual AffectedSubscriptions Create(Topic topic, ISubscriber subscriber)
        {
            if (!index.ContainsKey(topic))
            {
                index[topic] = new HashSet<ISubscriber>();
            }
            return PerformOperation(topic, subscriber, DefaultCondition(), InsertOperation());
        }

        public virtual AffectedSubscriptions Cancel(Topic topic, ISubscriber subscriber)
            => PerformOperation(topic, subscriber, DefaultCondition(), RemovalOperation());

        public virtual AffectedSubscriptions CancelAll(ISubscriber subscriber)
            => PerformOperation(null, subscriber, NoCondition(), RemovalOperation());

        public virtual ISet<ISubscriber> ForTopic(Topic topic)
        {
            var subscribers = new HashSet<ISubscriber>();
            foreach(var subscription in index)
            {
                var subscribedTopic = subscription.Key;
                if (subscribedTopic.Equals(topic) || subscribedTopic.IsSubTopic(topic))
                {
                    subscription.Value.ToList().ForEach(x => subscribers.Add(x));
                }
            };

            return subscribers;
        }

        private Operation InsertOperation()
            => (existingValues, givenValue) => existingValues.Add(givenValue);

        private Operation RemovalOperation()
            => (existingValues, givenValue) => existingValues.Remove(givenValue);

        private Condition DefaultCondition()
            => (subscription, topic, subscriber) => subscription.Key.Equals(topic);

        private Condition NoCondition()
            => (subscription, topic, subscriber) => true;

        private AffectedSubscriptions PerformOperation(
            Topic topic,
            ISubscriber subscriber,
            Condition condition,
            Operation operation)
        {
            var affectedSubscriptions = new AffectedSubscriptions();
            foreach (var subscription in index)
            {
                if (condition.Should(subscription, topic, subscriber) && operation.Perform(subscription.Value, subscriber))
                {
                    affectedSubscriptions.Add(topic, subscriber);
                }
            }

            return affectedSubscriptions;
        }
    }
}
