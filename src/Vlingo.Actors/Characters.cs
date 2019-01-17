// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    public class Characters<T>
    {
        private int current;
        private readonly T[] states;

        public Characters(IList<T> states)
        {
            this.states = states.ToArray();
            current = 0;
        }

        public int Become(int which)
        {
            if (which < 0 || which >= states.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(which), "Invalid state");
            }
            if (states[which] == null)
            {
                throw new ArgumentOutOfRangeException($"The state {which} is null.");
            }
            var previous = current;
            current = which;
            return previous;
        }

        public T Current => states[current];
    }
}
