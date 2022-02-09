// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace Vlingo.Xoom.Actors.TestKit
{
    public class TestState
    {
        private readonly IDictionary<string, object> _state;

        public TestState() => _state = new Dictionary<string, object>();

        public TestState PutValue(string name, object value)
        {
            _state[name] = value;
            return this;
        }

        public T ValueOf<T>(string name)
        {
            _state.TryGetValue(name, out object? value);
            return (T) value!;
        }
    }
}
