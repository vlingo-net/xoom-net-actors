// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Actors.Tests;

public class MailboxConfigurationTest : IDisposable
{
    private static readonly string MailboxName = "testConfigurationMailbox";
    private static readonly string PluginNamePerfix = "plugin.name.";
    private static readonly string PropertyNamePrefix = "plugin." + MailboxName;

    private readonly World _world;

    [Fact]
    public void TestArrayQueueConfiguration()
    {
        var classname = "Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue.ManyToOneConcurrentArrayQueuePlugin";

        var arrayQueueConfiguration = IMailboxConfiguration<IArrayQueueConfiguration>.ArrayQueueConfiguration();

        Assert.NotNull(arrayQueueConfiguration);

        arrayQueueConfiguration.WithMailboxName(MailboxName);
        arrayQueueConfiguration.MailboxImplementationClassname(classname);
        arrayQueueConfiguration.DefaultMailbox(true);
        arrayQueueConfiguration.Size(33333);
        arrayQueueConfiguration.FixedBackoff(99);
        arrayQueueConfiguration.DispatcherThrottlingCount(7);
        arrayQueueConfiguration.NotifyOnSend(true);
        arrayQueueConfiguration.SendRetires(14);

        var properties = arrayQueueConfiguration.ToProperties();

        Assert.Equal("true", properties.GetProperty(PluginNamePerfix + MailboxName));
        Assert.Equal(classname, properties.GetProperty(PropertyNamePrefix + ".classname"));
        Assert.Equal("True", properties.GetProperty(PropertyNamePrefix + ".defaultMailbox"));
        Assert.Equal("33333", properties.GetProperty(PropertyNamePrefix + ".size"));
        Assert.Equal("99", properties.GetProperty(PropertyNamePrefix + ".fixedBackoff"));
        Assert.Equal("7", properties.GetProperty(PropertyNamePrefix + ".dispatcherThrottlingCount"));
        Assert.Equal("True", properties.GetProperty(PropertyNamePrefix + ".notifyOnSend"));
        Assert.Equal("14", properties.GetProperty(PropertyNamePrefix + ".sendRetires"));

        _world.RegisterMailboxType(arrayQueueConfiguration);

        var greeter = _world.Stage.ActorFor<IGreeter>(
                        Definition.Has<GreeterActor>(
                            Definition.NoParameters,
                            MailboxName,
                            "test-mailbox"));

        Assert.Equal("hello, world", greeter.WithHello("world").Await());
    }

    [Fact]
    public void TestConcurrentQueueConfiguration()
    {
        var classname = "Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue.ConcurrentQueueMailboxPlugin";

        var concurrentQueueConfiguration = IMailboxConfiguration<IConcurrentQueueConfiguration>.ConcurrentQueueConfiguration();

        Assert.NotNull(concurrentQueueConfiguration);

        concurrentQueueConfiguration.WithMailboxName(MailboxName);
        concurrentQueueConfiguration.MailboxImplementationClassname(classname);
        concurrentQueueConfiguration.DefaultMailbox(true);
        concurrentQueueConfiguration.DispatcherThrottlingCount(7);
        concurrentQueueConfiguration.NumberOfDispatchersFactor(2);
        concurrentQueueConfiguration.NumberOfDispatchers(0);

        var properties = concurrentQueueConfiguration.ToProperties();

        Assert.Equal("true", properties.GetProperty(PluginNamePerfix + MailboxName));
        Assert.Equal(classname, properties.GetProperty(PropertyNamePrefix + ".classname"));
        Assert.Equal("True", properties.GetProperty(PropertyNamePrefix + ".defaultMailbox"));
        Assert.Equal("2", properties.GetProperty(PropertyNamePrefix + ".numberOfDispatchersFactor"));
        Assert.Equal("0", properties.GetProperty(PropertyNamePrefix + ".numberOfDispatchers"));

        _world.RegisterMailboxType(concurrentQueueConfiguration);

        var greeter = _world.Stage.ActorFor<IGreeter>(
            Definition.Has<GreeterActor>(
                Definition.NoParameters,
                MailboxName,
                "test-mailbox"));

        Assert.Equal("hello, world", greeter.WithHello("world").Await());
    }

    [Fact]
    public void TestSharedRingBufferConfiguration()
    {
        var classname = "Vlingo.Xoom.Actors.Plugin.Mailbox.SharedRingBuffer.SharedRingBufferMailboxPlugin";

        var sharedRingBufferConfiguration = IMailboxConfiguration<ISharedRingBufferConfiguration>.SharedRingBufferConfiguration();

        Assert.NotNull(sharedRingBufferConfiguration);

        sharedRingBufferConfiguration.WithMailboxName(MailboxName);
        sharedRingBufferConfiguration.MailboxImplementationClassname(classname);
        sharedRingBufferConfiguration.DefaultMailbox(true);
        sharedRingBufferConfiguration.Size(33333);
        sharedRingBufferConfiguration.FixedBackoff(99);
        sharedRingBufferConfiguration.DispatcherThrottlingCount(7);
        sharedRingBufferConfiguration.NotifyOnSend(true);

        var properties = sharedRingBufferConfiguration.ToProperties();

        Assert.Equal("true", properties.GetProperty(PluginNamePerfix + MailboxName));
        Assert.Equal(classname, properties.GetProperty(PropertyNamePrefix + ".classname"));
        Assert.Equal("True", properties.GetProperty(PropertyNamePrefix + ".defaultMailbox"));
        Assert.Equal("33333", properties.GetProperty(PropertyNamePrefix + ".size"));
        Assert.Equal("99", properties.GetProperty(PropertyNamePrefix + ".fixedBackoff"));
        Assert.Equal("7", properties.GetProperty(PropertyNamePrefix + ".dispatcherThrottlingCount"));
        Assert.Equal("True", properties.GetProperty(PropertyNamePrefix + ".notifyOnSend"));

        _world.RegisterMailboxType(sharedRingBufferConfiguration);

        var greeter = _world.Stage.ActorFor<IGreeter>(
            Definition.Has<GreeterActor>(
                Definition.NoParameters,
                MailboxName,
                "test-mailbox"));

        Assert.Equal("hello, world", greeter.WithHello("world").Await());
    }

    public MailboxConfigurationTest(ITestOutputHelper output)
    {
        var converter = new Converter(output);
        Console.SetOut(converter);
        _world = World.StartWithDefaults("mailbox-configuration-test");
    }

    public void Dispose()
    {
        _world.Terminate();
    }
}

public interface IGreeter
{
    public static string Hello = "hello, ";

    ICompletes<string> WithHello(string to);
}

public class GreeterActor : Actor, IGreeter
{
    public ICompletes<string> WithHello(string to) => Completes().With(IGreeter.Hello + to);
}