// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using Vlingo.Xoom.Actors.Plugin;
using Vlingo.Xoom.Actors.Plugin.Completes;
using Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue;
using Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox;

public class MailboxTypeTest : ActorsTest
{
    [Fact]
    public void ManyToOneQueueMailboxTest()
    {
        var properties = new Properties();
        properties.SetProperty("plugin.name.testArrayQueueMailbox", "true");
        properties.SetProperty("plugin.testArrayQueueMailbox.classname", "Vlingo.Xoom.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue.ManyToOneConcurrentArrayQueuePlugin");
        properties.SetProperty("plugin.testArrayQueueMailbox.defaultMailbox", "false");

        var mailboxPlugin = new ManyToOneConcurrentArrayQueuePlugin();
        var pluginProperties = new PluginProperties("testArrayQueueMailbox", properties);
        var completesPlugin = new PooledCompletesPlugin();

        completesPlugin.Configuration.BuildWith(World.Configuration, pluginProperties);
        mailboxPlugin.Configuration.BuildWith(World.Configuration, pluginProperties);

        mailboxPlugin.Start(World);

        var empty = World.ActorFor<IEmpty>(Definition.Has<EmptyActor>(new List<object>(), "testArrayQueueMailbox", "empty"));

        Assert.Equal(typeof(ManyToOneConcurrentArrayQueueMailbox), World.Stage.MailboxTypeOf(empty));
        Assert.Equal("ManyToOneConcurrentArrayQueueMailbox", World.Stage.MailboxTypeNameOf(empty));
    }

    [Fact]
    public void ConcurrentQueueMailboxTest()
    {
        var properties = new Properties();
        properties.SetProperty("plugin.name.testConcurrentQueueMailbox", "true");
        properties.SetProperty("plugin.testConcurrentQueueMailbox.classname", "Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue.ConcurrentQueueMailboxPlugin");
        properties.SetProperty("plugin.testConcurrentQueueMailbox.defaultMailbox", "false");

        var mailboxPlugin = new ConcurrentQueueMailboxPlugin("test");
        var pluginProperties = new PluginProperties("testConcurrentQueueMailbox", properties);
        var completesPlugin = new PooledCompletesPlugin();

        completesPlugin.Configuration.BuildWith(World.Configuration, pluginProperties);
        mailboxPlugin.Configuration.BuildWith(World.Configuration, pluginProperties);

        mailboxPlugin.Start(World);

        var empty = World.ActorFor<IEmpty>(Definition.Has<EmptyActor>(new List<object>(), "testConcurrentQueueMailbox", "empty"));

        Assert.Equal(typeof(ConcurrentQueueMailbox), World.Stage.MailboxTypeOf(empty));
        Assert.Equal("ConcurrentQueueMailbox", World.Stage.MailboxTypeNameOf(empty));
    }
    public interface IEmpty {}

    public class EmptyActor: Actor, IEmpty { }
}