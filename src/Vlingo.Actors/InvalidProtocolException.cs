// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
    public class InvalidProtocolException : Exception
    {
        private const long SerialVersionID = 1L;

        public InvalidProtocolException(string protocolName, IList<Failure> failures)
            : base(ToReadableMessage(protocolName, failures))
        {
        }

        private static string ToReadableMessage(string protocolName, IList<Failure> failures)
            => string.Format(
                "For protocol {0}\n{1}",
                protocolName,
                string.Join("\n", failures.Select(f => $"\t{f.ToString()}")));

        public class Failure
        {
            private readonly string method;
            private readonly string cause;

            public Failure(string method, string cause)
            {
                this.method = method;
                this.cause = cause;
            }

            public override string ToString()
                => $"In method `{method}`: \n\t\t{cause}";
        }
    }
}
