// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public sealed class DeadLetter
    {
        private readonly Actor actor;
        private readonly string representation;

        public DeadLetter(Actor actor, string representation)
        {
            this.actor = actor;
            this.representation = representation;
        }

        public string Representation => representation;

        public override string ToString() => $"DeadLetter[{actor}.{representation}]";
    }
}
