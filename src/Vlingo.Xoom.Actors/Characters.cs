// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Actors
{
    public class Characters<T>
    {
        private int _current;
        private readonly T[] _states;

        public Characters(IList<T> states)
        {
            _states = states.ToArray();
            _current = 0;
        }

        public int Become(int which)
        {
            if (which < 0 || which >= _states.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(which), "Invalid state");
            }
            if (_states[which] == null)
            {
                throw new ArgumentOutOfRangeException($"The state {which} is null.");
            }
            var previous = _current;
            _current = which;
            return previous;
        }

        public T Current => _states[_current];
    }
}
