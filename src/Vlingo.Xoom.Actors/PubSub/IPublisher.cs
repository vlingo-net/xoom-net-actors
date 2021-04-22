// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.PubSub
{
    public interface IPublisher
    {
        void Publish(Topic topic, IMessage message);
        bool Subscribe(Topic topic, ISubscriber subscriber);
        bool Unsubscribe(Topic topic, ISubscriber subscriber);
        void UnsubscribeAllTopics(ISubscriber subscriber);
    }
}
