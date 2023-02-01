// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ProxyLookupTest : ActorsTest
    {
        [Fact]
        public void ShouldLoadGenericProxyWhenAvailable()
        {
            var actor = World.ActorFor<IGenericInterface<int, string>>(typeof(GenericInterfaceActor));

            Assert.True(actor.GetType().IsGenericType);

            var actualProxyType = actor.GetType().GetGenericTypeDefinition();
            var expectedProxyType = typeof(GenericInterface__Proxy<,>);

            Assert.Equal(expectedProxyType, actualProxyType);
        }

        [Fact]
        public void ShouldGenerateProxyWhenNotAvailable()
        {
            var actor = World.ActorFor<INoProxyGenericInterface<int, string, bool>>(typeof(GenericInterfaceActorWithNoProxy));

            Assert.True(actor.GetType().IsGenericType);

            var actualProxyType = actor.GetType().GetGenericTypeDefinition();

            Assert.Equal(3, actualProxyType.GetGenericArguments().Length);
        }
        
        [Fact]
        public void ShouldLoadGenericMethodProxyWhenNotAvailable()
        {
            var actor = World.ActorFor<IGenericMethod>(typeof(GenericMethodActorWithNoProxy));

            var actualProxyType = actor.GetType();
            var methodType = actualProxyType.GetMethod("Handle");
            var methodGenericDefinition = methodType.GetGenericMethodDefinition();

            Assert.Single(methodGenericDefinition.GetGenericArguments());
        }
        
        [Fact]
        public void ShouldLoadGenericProxyWithConstraintsWhenNotAvailable()
        {
            var actor = World.ActorFor<IGenericWithConstraints<GenericHandler, object>>(typeof(GenericInterfaceWithContraintsNoProxy));

            Assert.True(actor.GetType().IsGenericType);
            
            var actualProxyType = actor.GetType().GetGenericTypeDefinition();

            Assert.Equal(2, actualProxyType.GetGenericArguments().Length);
        }

        public ProxyLookupTest(ITestOutputHelper output)
        {
            var converter = new Converter(output);
            Console.SetOut(converter);
        }

        private class GenericInterfaceActor : Actor, IGenericInterface<int, string>
        {

            public void DoSomething(int a, string b)
            {
            }
        }
        private class GenericInterfaceActorWithNoProxy : Actor, INoProxyGenericInterface<int, string, bool>
        {
            public void DoSomethingElse(int a, string b, bool c)
            {
            }
        }
        
        private class GenericMethodActorWithNoProxy : Actor, IGenericMethod
        {
            public void Handle<T>(string name, Action<T> consumer) where T : GenericHandler
            {
            }
        }
        
        private class GenericInterfaceWithContraintsNoProxy : Actor, IGenericWithConstraints<GenericHandler, object>
        {
            public object Handle(Action<GenericHandler> consumer)
            {
                return new object();
            }
        }
    }

    public interface IGenericInterface<T, R>
    {
        void DoSomething(T a, R b);
    }

    public interface INoProxyGenericInterface<T, R, S>
    {
        void DoSomethingElse(T a, R b, S c);
    }

    public class GenericInterface__Proxy<T, R> : IGenericInterface<T, R>
    {
        public GenericInterface__Proxy(Actor actor, IMailbox mailbox)
        {
        }

        public void DoSomething(T a, R b)
        {
        }
    }

    public interface IGenericMethod
    {
        void Handle<T>(string name, Action<T> consumer) where T : GenericHandler;
    }

    public interface IGenericWithConstraints<T, R> where T : GenericHandler where R : new()
    {
        R Handle(Action<T> consumer);
    }
    
    public class GenericHandler {}
}
