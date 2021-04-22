// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    public sealed class DeadLetter
    {
        private readonly Actor _actor;
        private readonly string _representation;

        public DeadLetter(Actor actor, string representation)
        {
            _actor = actor;
            _representation = representation;
        }

        public string Representation => _representation;

        public override string ToString() => $"DeadLetter[{_actor}.{_representation}]";
    }
}
