// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    /// <summary>
    /// Routee represents a potential target for for a routed message.
    /// </summary>
    public class Routee
    {
        private readonly Actor actor;

        internal static IList<Routee> ForAll(IList<Actor> children)
            => children
                .Select(x => new Routee(x))
                .ToList();

        internal Routee(Actor actor) : base()
        {
            this.actor = actor;
        }

        public virtual int PendingMessages
            => actor.LifeCycle.Environment.Mailbox.PendingMessages;

        public virtual T As<T>() => actor.SelfAs<T>();
    }
}
