// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests;

public class ActorProxyTest
{
    private static readonly AtomicReference<string> UnderTest = new("UNKNOWN");
    private static readonly CountdownEvent Latch = new(1);

    private readonly Thread _runtimeStartWorldThread = new(() =>
    {
        TestRuntimeDiscoverer.IsTestDetectionDeactivated = true;
        var world = World.StartWithDefaults("StartForMain");
        UnderTest.Set(world.ResolveDynamic<bool>(ActorProxy.InternalActorProxyForTestId) ? "TRUE" : "FALSE");
        Latch.Signal();
        TestRuntimeDiscoverer.IsTestDetectionDeactivated = false;
    });

    [Fact]
    public void TestThatActorProxyInitializesForMain()
    {
        Assert.False(ActorProxy.IsInitializingForTest()); // state before initializing

        // use a separate Thread since it will not be on this stack
        _runtimeStartWorldThread.Start();

        Latch.Wait();

        Assert.Equal("FALSE", UnderTest.Get());
    }

    [Fact]
    public void TestThatActorProxyInitializesForTest()
    {
        Assert.False(ActorProxy.IsInitializingForTest()); // state before initializing

        var world = World.StartWithDefaults("StartForTest");

        Assert.True(world.ResolveDynamic<bool>(ActorProxy.InternalActorProxyForTestId));
    }
}