// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.PubSub;

public class DefaultPublisher : IPublisher
{
    private readonly Subscriptions _subscriptions;

    public DefaultPublisher() => _subscriptions = new Subscriptions();

    public virtual void Publish(Topic topic, IMessage message)
    {
        foreach(var subscriber in _subscriptions.ForTopic(topic))
        {
            subscriber.Receive(message);
        }
    }

    public virtual bool Subscribe(Topic topic, ISubscriber subscriber)
    {
        var affectedSubscriptions = _subscriptions.Create(topic, subscriber);
        return affectedSubscriptions.HasAny;
    }

    public virtual bool Unsubscribe(Topic topic, ISubscriber subscriber)
    {
        var affectedSubscriptions = _subscriptions.Cancel(topic, subscriber);
        return affectedSubscriptions.HasAny;
    }

    public virtual void UnsubscribeAllTopics(ISubscriber subscriber)
        => _subscriptions.CancelAll(subscriber);
}