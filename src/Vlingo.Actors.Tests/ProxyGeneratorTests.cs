// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
            Assert.Contains("private const string DoSomethingRepresentation1 = \"DoSomething(System.Threading.Tasks.Task)\";", result.Source);
            Assert.Contains("System.Threading.Tasks.Task t", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenTestInterface> cons128873 = __ => __.DoSomething(t);", result.Source);
        }

        [Fact]
        public void ShouldNotIncludeGenericConstraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithoutConstraint));
            Assert.Contains("private const string WriteRepresentation1 = \"Write<TSource>(string, int)\";", result.Source);
            Assert.Contains("public void Write<TSource>(string id, int state)", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenericMethodWithoutConstraint> cons128873 = __ => __.Write<TSource>(id, state);", result.Source);
        }

        [Fact]
        public void ShouldCorrectlyGenerateParameterNameWithReservedKeywords()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenTestReservedInterface));
            Assert.Contains("private const string DoSomethingRepresentation1 = \"DoSomething(object)\";", result.Source);
            Assert.Contains("public void DoSomething(object @object)", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenTestReservedInterface> cons128873 = __ => __.DoSomething(@object);", result.Source);
        }

        [Fact]
        public void ShouldCorrectlyGenerateParameterNameWithReservedKeywordsAddtional()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenTestReservedKeyword));
            Assert.Contains("private const string DoSomethingRepresentation1 = \"DoSomething(object, bool)\";", result.Source);
            Assert.Contains("public void DoSomething(object @object, bool @event)", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenTestReservedKeyword> cons128873 = __ => __.DoSomething(@object, @event);", result.Source);
        }

        [Fact]
        public void ShouldIncludeGenericConstraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithConstraint));
            Assert.Contains("private const string WriteRepresentation1 = \"Write<TSource>(string, int)\";", result.Source);
            Assert.Contains("public void Write<TSource>(string id, int state) where TSource : Vlingo.Actors.Tests.IProxyGenTestInterface", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenericMethodWithConstraint> cons128873 = __ => __.Write<TSource>(id, state);", result.Source);
        }

        [Fact]
        public void ShouldIncludeMultipleGenericConstraintForMethod()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IProxyGenericMethodWithMultipleConstraint));
            Assert.Contains("private const string WriteRepresentation1 = \"Write<TSource>(string, int)\";", result.Source);
            Assert.Contains("public void Write<TSource>(string id, int state) where TSource : Vlingo.Actors.Tests.IProxyGenTestInterface, Vlingo.Actors.Tests.IProxyGenTestSecondInterface", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.IProxyGenericMethodWithMultipleConstraint> cons128873 = __ => __.Write<TSource>(id, state);", result.Source);
        }

        [Fact]
        public void ShouldCreateAProxyWithInnerInterface()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(ClassWithNestedInterface.INestedInterface));
            Assert.Contains("private const string DoSomethingRepresentation1 = \"DoSomething()\";", result.Source);
            Assert.Contains("public class NestedInterface__Proxy : Vlingo.Actors.Tests.ClassWithNestedInterface.INestedInterface", result.Source);
            Assert.Contains("Action<Vlingo.Actors.Tests.ClassWithNestedInterface.INestedInterface> cons128873 = __ => __.DoSomething();", result.Source);
        }

        [Fact]
        public void ShouldCreateAProxyWithReferenceToInnerInterface()
        {
            var generator = ProxyGenerator.ForTest(false, World.DefaultLogger);
            var result = generator.GenerateFor(typeof(IOuterInnerClassInterface));
            Assert.Contains("public class OuterInnerClassInterface__Proxy : Vlingo.Actors.Tests.IOuterInnerClassInterface", result.Source);
            Assert.Contains("public void DoSomethingWith(Vlingo.Actors.Tests.OuterClass.InnerClass obj)", result.Source);
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

    public interface IProxyGenTestReservedKeyword
    {
        void DoSomething(object @object, bool @event);
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

    public class ClassWithNestedInterface
    {
        public interface INestedInterface
        {
            void DoSomething();
        }
    }

    public class ActorForNestedInterface : Actor, ClassWithNestedInterface.INestedInterface
    {
        public void DoSomething() { }
    }

    public class OuterClass
    {
        public class InnerClass
        {
        }
    }

    public interface IOuterInnerClassInterface
    {
        void DoSomethingWith(OuterClass.InnerClass obj);
    }

    public class ActorForNestedClass : Actor, IOuterInnerClassInterface
    {
        public void DoSomethingWith(OuterClass.InnerClass obj) { }
    }
}