// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.PubSub;

public abstract class Topic
{
    public Topic(string name)
    {
        Name = name;
    }

    public virtual string Name { get; }

    public abstract bool IsSubTopic(Topic anotherTopic);

    public override bool Equals(object? obj)
    {
        if(obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var otherTopic = (Topic)obj;

        return string.Equals(Name, otherTopic.Name);
    }

    public override int GetHashCode()
        => $"Topic[{GetType().FullName}]-{Name}".GetHashCode();
}