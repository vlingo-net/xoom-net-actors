// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Xoom.Actors;

public class InvalidProtocolException : Exception
{
    private const long SerialVersionId = 1L;

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
        private readonly string _method;
        private readonly string _cause;

        public Failure(string method, string cause)
        {
            _method = method;
            _cause = cause;
        }

        public override string ToString()
            => $"In method `{_method}`: \n\t\t{_cause}";
    }
}