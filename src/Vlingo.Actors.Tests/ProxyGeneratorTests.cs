// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
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
            Assert.Contains("System.Threading.Tasks.Task t", result.Source);
        }

        [Fact]
        public void ShouldNotIncludeGenericContraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithoutConstraint));
            Assert.Contains("public void Write<TSource>(string id, int state)", result.Source);
        }

        [Fact]
        public void ShouldCorrectlyGenerateParameterName()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenTestReservedInterface));
            Assert.Contains("public void DoSomething(object @object)", result.Source);
        }

        [Fact]
        public void ShouldIncludeGenericContraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithConstraint));
            Assert.Contains("public void Write<TSource>(string id, int state) where TSource : Vlingo.Actors.Tests.IProxyGenTestInterface", result.Source);
        }

        [Fact]
        public void ShouldIncludeMultipleGenericContraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithMultipleConstraint));
            Assert.Contains("public void Write<TSource>(string id, int state) where TSource : Vlingo.Actors.Tests.IProxyGenTestInterface, Vlingo.Actors.Tests.IProxyGenTestSecondInterface", result.Source);
        }

        [Fact]
        public void ShouldCreateAProxyWithInnerInterface()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(MyClass.IMyInterface));
            Assert.Contains("public class MyInterface__Proxy : Vlingo.Actors.Tests.MyClass.IMyInterface", result.Source);
        }

        [Fact]
        public void ShouldCreateAProxyWithReferenceToInnerInterface()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IMyInterface));
            Assert.Contains("public class MyInterface__Proxy : Vlingo.Actors.Tests.IMyInterface", result.Source);
            Assert.Contains("public void DoSomethingWith(Vlingo.Actors.Tests.MyOuterClass.MyInnerClass obj)", result.Source);
        }
    }

    public interface IProxyGenTestInterface
    {
        void DoSomething(Task t);
    }

    public interface IProxyGenTestSecondInterface
    {
        void DoSomething(Task t);
    }

    public interface IProxyGenTestReservedInterface
    {
        void DoSomething(object @object);
    }

    public interface IProxyGenericMethodWithoutConstraint
    {
        void Write<TSource>(string id, int state);
    }

    public interface IProxyGenericMethodWithConstraint
    {
        void Write<TSource>(string id, int state) where TSource : IProxyGenTestInterface;
    }

    public interface IProxyGenericMethodWithMultipleConstraint
    {
        void Write<TSource>(string id, int state) where TSource : IProxyGenTestInterface, IProxyGenTestSecondInterface;
    }

    public class MyClass
    {
        public interface IMyInterface
        {
            void DoSomething();
        }
    }

    public class MyActor : Actor, MyClass.IMyInterface
    {
        public void DoSomething() { }
    }


    public class MyOuterClass
    {
        public class MyInnerClass
        {
        }
    }

    public interface IMyInterface
    {
        void DoSomethingWith(MyOuterClass.MyInnerClass obj);
    }

    public class MySecondActor : Actor, IMyInterface
    {
        public void DoSomethingWith(MyOuterClass.MyInnerClass obj) { }
    }
}