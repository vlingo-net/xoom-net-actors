// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Actors.Tests
{
    public class Converter : TextWriter
    {
        private readonly ITestOutputHelper _output;
        
        public Converter(ITestOutputHelper output) => _output = output;

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string message)
        {
            try
            {
                _output.WriteLine(message);
            }
            catch (InvalidOperationException e)
            {
                if (e.Message != "There is no currently active test.")
                {
                    throw;
                }
            }
        }

        public override void WriteLine(string format, params object[] args) => _output.WriteLine(format, args);
    }
}