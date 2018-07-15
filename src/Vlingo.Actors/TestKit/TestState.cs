// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System.Collections.Generic;

namespace Vlingo.Actors.TestKit
{
    public class TestState
    {
        private readonly IDictionary<string, object> state;

        public TestState()
        {
            state = new Dictionary<string, object>();
        }

        public TestState PutValue(string name, object value)
        {
            state[name] = value;
            return this;
        }

        public T ValueOf<T>(string name)
        {
            return (T)state[name];
        }
    }
}
