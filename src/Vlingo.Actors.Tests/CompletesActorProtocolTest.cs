// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Vlingo.Common;
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
        private readonly TestUntil untilHello;
        private readonly TestUntil untilOne;

        public CompletesActorProtocolTest()
        {
            untilHello = TestUntil.Happenings(1);
            untilOne = TestUntil.Happenings(1);
        }

        public override void Dispose()
        {
            untilHello.Dispose();
            untilOne.Dispose();
            base.Dispose();
        }

        [Fact]
        public void TestReturnsCompletesForSideEffects()
        {
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesActor));

            uc.GetHello().AndThenConsume(hello => SetHello(hello.greeting));
            untilHello.Completes();
            Assert.Equal(Hello, greeting);

            uc.GetOne().AndThenConsume(value => SetValue(value));
            untilOne.Completes();
            Assert.Equal(1, value);
        }

        [Fact]
        public void TestAfterAndThenCompletesForSideEffects()
        {
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesActor));
            var helloCompletes = uc.GetHello();
            helloCompletes
                .AndThen(hello => new Hello(Prefix + hello.greeting))
                .AndThenConsume(hello => SetHello(hello.greeting));
            untilHello.Completes();

            Assert.NotEqual(Hello, helloCompletes.Outcome.greeting);
            Assert.NotEqual(Hello, greeting);
            Assert.Equal(Prefix + Hello, helloCompletes.Outcome.greeting);
            Assert.Equal(Prefix + Hello, greeting);

            var one = uc.GetOne();
            one
                .AndThen(value => value + 1)
                .AndThenConsume(value => SetValue(value));
            untilOne.Completes();

            Assert.NotEqual(1, one.Outcome);
            Assert.NotEqual(1, value);
            Assert.Equal(2, one.Outcome);
            Assert.Equal(2, value);
        }

        [Fact(Skip = "Need explanation of why it should timeout")]
        public void TestThatTimeOutOccursForSideEffects()
        {
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesCausesTimeoutActor));
            var helloCompletes = uc.GetHello()
                .AndThenConsume(TimeSpan.FromMilliseconds(2), new Hello(HelloNot), hello => SetHello(hello.greeting))
                .Otherwise(failedHello =>
                {
                    SetHello(failedHello.greeting);
                    return failedHello;
                });

            untilHello.Completes();

            Assert.NotEqual(Hello, greeting);
            Assert.Equal(HelloNot, helloCompletes.Outcome.greeting);

            var oneCompletes = uc.GetOne()
                .AndThenConsume(TimeSpan.FromMilliseconds(2), 0, value => SetValue(value))
                .Otherwise(value =>
                {
                    untilOne.Happened();
                    return 0;
                });

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

        public override string ToString() => $"Hello[{greeting}]";
    }

    public interface IUsesCompletes
    {
        ICompletes<Hello> GetHello();
        ICompletes<int> GetOne();
    }

    public class UsesCompletesActor : Actor, IUsesCompletes
    {
        public ICompletes<Hello> GetHello() => Completes().With(new Hello(CompletesActorProtocolTest.Hello));

        public ICompletes<int> GetOne() => Completes().With(1);
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

            return Completes().With(new Hello(CompletesActorProtocolTest.Hello));
        }

        public ICompletes<int> GetOne()
        {
            try
            {
                Thread.Sleep(100);
            }
            catch (ThreadInterruptedException) { }

            return Completes().With(1);
        }
    }
}