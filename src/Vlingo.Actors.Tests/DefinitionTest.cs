// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class DefinitionTest : ActorsTest
    {
        [Fact]
        public void TestDefinitionHasTypeNoParameters()
        {
            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters);

            Assert.NotNull(definition);
            Assert.Null(definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.Null(definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }

        [Fact]
        public void TestDefinitionHasTypeParameters()
        {
            var definition = Definition.Has<TestInterfaceActor>(Definition.Parameters("text", 1));

            Assert.NotNull(definition);
            Assert.Null(definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Equal(2, definition.Parameters().Count);
            Assert.Equal("text", definition.Parameters()[0]);
            Assert.Equal(1, (int)definition.Parameters()[1]);
            Assert.Null(definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }

        [Fact]
        public void TestDefinitionHasTypeNoParametersActorName()
        {
            const string actorName = "test-actor";

            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters, actorName);

            Assert.NotNull(definition);
            Assert.NotNull(definition.ActorName);
            Assert.Equal(actorName, definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.Null(definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }

        [Fact]
        public void TestDefinitionHasTypeNoParametersDefaultParentActorName()
        {
            const string actorName = "test-actor";

            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters, World.DefaultParent, actorName);

            Assert.NotNull(definition);
            Assert.NotNull(definition.ActorName);
            Assert.Equal(actorName, definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.NotNull(definition.Parent);
            Assert.Equal(World.DefaultParent, definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }

        [Fact]
        public void TestDefinitionHasTypeNoParametersParentActorName()
        {
            const string actorName = "test-actor";

            var parentHolder = new ParentHolder();

            World.ActorFor<IParentInterface>(Definition.Has<ParentInterfaceActor>(Definition.Parameters(parentHolder)));

            var definition = Definition.Has<TestInterfaceActor>(Definition.NoParameters, parentHolder.parent, actorName);

            Assert.NotNull(definition);
            Assert.NotNull(definition.ActorName);
            Assert.Equal(actorName, definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.NotNull(definition.Parent);
            Assert.Equal(parentHolder.parent, definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionHasTypeNoParametersParentActorName()
        {
            const string actorName = "test-actor";

            var parentHolder = new ParentHolder();

            World.ActorFor<IParentInterface>(Definition.Has(() => new ParentInterfaceActor(parentHolder)));

            var definition = Definition.Has<ITestInterface>(() => new TestInterfaceActor(), parentHolder.parent, actorName);

            Assert.NotNull(definition);
            Assert.NotNull(definition.ActorName);
            Assert.Equal(actorName, definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.NotNull(definition.Parent);
            Assert.Equal(parentHolder.parent, definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }

        [Fact]
        public void TestTypeSafeConstructorDefinitionThrowsWithFieldExpression()
        {
            const string actorName = "test-actor";
            var actor = new TestInterfaceActor();
            var message = Assert.Throws<ArgumentException>(() => Definition.Has(() => actor, actorName)).Message;
            Assert.Equal($"The create function must be a 'new T (parameters)' expression", message);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionThrowsWithConstantExpression()
        {
            var message = Assert.Throws<ArgumentException>(() => Definition.Has(() => 1)).Message;
            Assert.Equal($"The create function must be a 'new T (parameters)' expression", message);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionThrowsWithLogicalBooleanExpression()
        {
            var message = Assert.Throws<ArgumentException>(() => Definition.Has(() => false || true )).Message;
            Assert.Equal($"The create function must be a 'new T (parameters)' expression", message);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionThrowsWithPropertyExpression()
        {
            const string actorName = "test-actor";
            var actor = new TestInterfaceActor();
            var message = Assert.Throws<ArgumentException>(() => Definition.Has(() => actor.IsStopped, actorName)).Message;
            Assert.Equal($"The create function must be a 'new T (parameters)' expression", message);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionHasParameters()
        {
            const string actorName = "test-actor";
            
            var parentHolder = new ParentHolder();
            World.ActorFor<IParentInterface>(() => new ParentInterfaceActor(parentHolder));
            var definition = Definition.Has(() => new TestInterfaceActor(), parentHolder.parent, actorName);
            
            Assert.NotNull(definition);
            Assert.NotNull(definition.ActorName);
            Assert.Equal(actorName, definition.ActorName);
            Assert.Null(definition.MailboxName);
            Assert.NotNull(definition.Parameters());
            Assert.Empty(definition.Parameters());
            Assert.NotNull(definition.Parent);
            Assert.Equal(parentHolder.parent, definition.Parent);
            Assert.NotNull(definition.ParentOr(new TestInterfaceActor()));
            Assert.Equal(typeof(TestInterfaceActor), definition.Type);
        }
        
        [Fact]
        public void TestTypeSafeConstructorDefinitionThrowsWithActorNotImplementingProtocol()
        {
            var message = Assert.Throws<ArgumentException>(() => Definition.Has<ITestInterface>(() => new TestInterface())).Message;
            Assert.Equal($"The type '{typeof(TestInterface).FullName}' must be instance of an actor. Derive it from Actor class.", message);
        }

        public class ParentInterfaceActor : Actor, IParentInterface
        {
            public ParentInterfaceActor(ParentHolder parentHolder) { parentHolder.parent = this; }
        }

        public interface ITestInterface { }

        public class TestInterfaceActor : Actor, ITestInterface { }
        
        public class TestInterface : ITestInterface { }

        public class ParentHolder
        {
            public ParentInterfaceActor parent;
        }
    }
}
