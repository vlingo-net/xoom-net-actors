// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ProxyGeneratorTests : ActorsTest
    {
        [Fact]
        public void ShouldIncludeNamespacesForMethodParameters()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenTestInterface));
            Assert.Contains("using System.Net;", result.Source);
        }
    }

    public interface IProxyGenTestInterface
    {
        void DoSomething(System.Net.Dns dns);
    }
}
