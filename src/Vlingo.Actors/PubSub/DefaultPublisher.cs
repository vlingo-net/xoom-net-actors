// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.PubSub
{
    public class DefaultPublisher : IPublisher
    {
        private readonly Subscriptions subscriptions;

        public DefaultPublisher()
        {
            subscriptions = new Subscriptions();
        }

        public virtual void Publish(Topic topic, IMessage message)
        {
            foreach(var subscriber in subscriptions.ForTopic(topic))
            {
                subscriber.Receive(message);
            }
        }

        public virtual bool Subscribe(Topic topic, ISubscriber subscriber)
        {
            var affectedSubscriptions = subscriptions.Create(topic, subscriber);
            return affectedSubscriptions.HasAny;
        }

        public virtual bool Unsubscribe(Topic topic, ISubscriber subscriber)
        {
            var affectedSubscriptions = subscriptions.Cancel(topic, subscriber);
            return affectedSubscriptions.HasAny;
        }

        public virtual void UnsubscribeAllTopics(ISubscriber subscriber)
            => subscriptions.CancelAll(subscriber);
    }
}
