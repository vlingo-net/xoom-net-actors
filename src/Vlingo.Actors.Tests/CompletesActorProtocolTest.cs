// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class CompletesActorProtocolTest : ActorsTest
    {
        internal const string Hello = "Hello, Completes!";
        private const string HelloNot = "Bye!";
        private const string Prefix = "*** ";

        private string greeting;
        private int value;
        private readonly TestUntil untilHello = TestUntil.Happenings(1);
        private readonly TestUntil untilOne = TestUntil.Happenings(1);

        public override void Dispose()
        {
            untilHello.Dispose();
            untilOne.Dispose();
            base.Dispose();
        }

        [Fact]
        public void TestReturnsCompletes()
        {
            var uc = World.ActorFor<IUsesCompletes>(Definition.Has<UsesCompletesActor>(Definition.NoParameters));

            uc.GetHello().After(hello => SetHello(hello.greeting));
            untilHello.Completes();
            Assert.Equal(Hello, greeting);

            uc.GetOne().After(value => SetValue(value));
            untilOne.Completes();
            Assert.Equal(1, value);
        }

        [Fact]
        public void TestAfterAndThenCompletes()
        {
            var uc = World.ActorFor<IUsesCompletes>(Definition.Has<UsesCompletesActor>(Definition.NoParameters));
            var helloCompletes = uc.GetHello();
            helloCompletes
                .After(() => new Hello(Prefix + helloCompletes.Outcome.greeting))
                .AndThen(hello => SetHello(hello.greeting));
            untilHello.Completes();

            Assert.NotEqual(Hello, helloCompletes.Outcome.greeting);
            Assert.NotEqual(Hello, greeting);
            Assert.Equal(Prefix + Hello, helloCompletes.Outcome.greeting);
            Assert.Equal(Prefix + Hello, greeting);

            var one = uc.GetOne();
            one
                .After(() => one.Outcome + 1)
                .AndThen(value => SetValue(value));
            untilOne.Completes();

            Assert.NotEqual(1, one.Outcome);
            Assert.NotEqual(1, value);
            Assert.Equal(2, one.Outcome);
            Assert.Equal(2, value);
        }

        [Fact]
        public void TestThatTimeOutOccurs()
        {
            var uc = World.ActorFor<IUsesCompletes>(Definition.Has<UsesCompletesCausesTimeoutActor>(Definition.NoParameters));
            var helloCompletes = uc.GetHello()
                .After(hello => SetHello(hello.greeting), 2, new Hello(HelloNot));
            untilHello.Completes();

            Assert.NotEqual(Hello, greeting);
            Assert.Equal(HelloNot, helloCompletes.Outcome.greeting);

            var oneCompletes = uc.GetOne().After(value => SetValue(value), 2, 0);
            untilOne.Completes();

            Assert.NotEqual(1, value);
            Assert.Equal(0, oneCompletes.Outcome);
        }

        private void SetHello(string hello)
        {
            greeting = hello;
            untilHello.Happened();
        }

        private void SetValue(int value)
        {
            this.value = value;
            untilOne.Happened();
        }
    }

    public class Hello
    {
        public string greeting;

        public Hello(string greeting)
        {
            this.greeting = greeting;
        }
    }

    public interface IUsesCompletes
    {
        ICompletes<Hello> GetHello();
        ICompletes<int> GetOne();
    }

    public class UsesCompletesActor : Actor, IUsesCompletes
    {
        public ICompletes<Hello> GetHello() => Completes<Hello>().With(new Hello(CompletesActorProtocolTest.Hello));

        public ICompletes<int> GetOne() => Completes<int>().With(1);
    }

    public class UsesCompletesCausesTimeoutActor : Actor, IUsesCompletes
    {
        public ICompletes<Hello> GetHello()
        {
            try
            {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException) { }

            return Completes<Hello>().With(new Hello(CompletesActorProtocolTest.Hello));
        }

        public ICompletes<int> GetOne()
        {
            try
            {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException) { }

            return Completes<int>().With(1);
        }
    }
}