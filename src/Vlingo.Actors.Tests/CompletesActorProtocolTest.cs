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


        [Fact]
        public void TestReturnsCompletesForSideEffects()
        {
            var testResults = TestResults.AfterCompleting(2);
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesActor), testResults);

            uc.GetHello().AndThenConsume(hello => testResults.SetGreeting(hello.greeting));
            uc.GetOne().AndThenConsume(value => testResults.SetValue(value));

            Assert.Equal(Hello, testResults.GetGreeting());
            Assert.Equal(1, testResults.GetValue());
        }

        [Fact]
        public void TestAfterAndThenCompletesForSideEffects()
        {
            var greetingsTestResults = TestResults.AfterCompleting(1);

            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesActor), greetingsTestResults);
            var helloCompletes = uc.GetHello();
            helloCompletes
                .AndThen(hello => new Hello(Prefix + hello.greeting))
                .AndThenConsume(hello => greetingsTestResults.SetGreeting(hello.greeting));

            Assert.NotEqual(Hello, greetingsTestResults.GetGreeting());
            Assert.NotEqual(Hello, helloCompletes.Outcome.greeting);
            Assert.Equal(Prefix + Hello, greetingsTestResults.GetGreeting());
            Assert.Equal(Prefix + Hello, helloCompletes.Outcome.greeting);

            var valueTestResults = TestResults.AfterCompleting(1);
            var one = uc.GetOne();
            one
                .AndThen(value => value + 1)
                .AndThenConsume(value => valueTestResults.SetValue(value));

            Assert.NotEqual(1, valueTestResults.GetValue());
            Assert.NotEqual(1, one.Outcome);
            Assert.Equal(2, valueTestResults.GetValue());
            Assert.Equal(2, one.Outcome);
        }

        [Fact]
        public void TestThatVoidReturnTypeThrowsException()
        {
            var testResults = TestResults.AfterCompleting(1);
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesActor), testResults);
            uc.CompletesNotSupportedForVoidReturnType();
            Assert.True(testResults.GetExceptionThrown());
        }

        [Fact]
        public void TestThatTimeOutOccursForSideEffects()
        {
            var greetingsTestResults = TestResults.AfterCompleting(1);
            var uc = World.ActorFor<IUsesCompletes>(typeof(UsesCompletesCausesTimeoutActor), greetingsTestResults);
            var helloCompletes = uc.GetHello()
                .AndThenConsume(TimeSpan.FromMilliseconds(1), new Hello(HelloNot), hello => greetingsTestResults.SetGreeting(hello.greeting))
                .Otherwise<Hello>(failedHello =>
                {
                    greetingsTestResults.SetGreeting(failedHello.greeting, true);
                    return failedHello;
                });
            
            Assert.NotEqual(Hello, greetingsTestResults.GetGreeting(true));
            Assert.Equal(HelloNot, helloCompletes.Outcome.greeting);

            var valueTestResults = TestResults.AfterCompleting(1);

            var oneCompletes = uc.GetOne()
                .AndThenConsume(TimeSpan.FromMilliseconds(1), 0, value => valueTestResults.SetValue(value))
                .Otherwise<int>(value =>
                {
                    valueTestResults.SetValue(value, true);
                    return 0;
                });

            var thread = new Thread(() =>
            {
                Thread.Sleep(100);
                oneCompletes.With(1); 
            });
            thread.Start();

            Assert.NotEqual(1, valueTestResults.GetValue(true));
            Assert.Equal(0, valueTestResults.GetValue(true));
            Assert.Equal(0, oneCompletes.Outcome);
        }

        private class UsesCompletesActor : Actor, IUsesCompletes
        {
            private readonly TestResults results;

            public UsesCompletesActor(TestResults results)
            {
                this.results = results;
            }

            public void CompletesNotSupportedForVoidReturnType()
            {
                try
                {
                    Completes().With("Must throw exception");
                    results.received.WriteUsing("exceptionThrown", false);
                }
                catch (Exception)
                {
                    results.received.WriteUsing("exceptionThrown", true);
                }
            }

            public ICompletes<Hello> GetHello() => Completes().With(new Hello(Hello));

            public ICompletes<int> GetOne() => Completes().With(1);
        }

        private class UsesCompletesCausesTimeoutActor : Actor, IUsesCompletes
        {
            private readonly TestResults results;

            public UsesCompletesCausesTimeoutActor(TestResults results)
            {
                this.results = results;
            }

            public void CompletesNotSupportedForVoidReturnType()
            {
                try
                {
                    Completes().With("Must throw exception");
                    results.received.WriteUsing("exceptionThrown", false);
                }
                catch
                {
                    results.received.WriteUsing("exceptionThrown", true);
                }
            }

            public ICompletes<Hello> GetHello()
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch (ThreadInterruptedException) { }

                return Completes().With(new Hello(Hello));
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

        private class TestResults
        {
            private readonly AtomicReference<string> receivedGreeting = new AtomicReference<string>(null);
            private readonly AtomicReference<string> receivedFailedGreeting = new AtomicReference<string>(null);
            private readonly AtomicInteger receivedValue = new AtomicInteger(0);
            private readonly AtomicInteger receivedFailedValue = new AtomicInteger(0);
            internal readonly AccessSafely received;
            private readonly AtomicBoolean exceptionThrown = new AtomicBoolean(false);

            public TestResults(AccessSafely received)
            {
                this.received = received;
            }

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.received.WritingWith<string>("receivedGreeting", s => testResults.receivedGreeting.Set(s));
                testResults.received.ReadingWith("receivedGreeting", testResults.receivedGreeting.Get);
                testResults.received.WritingWith<string>("receivedFailedGreeting", s => testResults.receivedFailedGreeting.Set(s));
                testResults.received.ReadingWith("receivedFailedGreeting", testResults.receivedFailedGreeting.Get);
                testResults.received.WritingWith<int>("receivedValue", v => testResults.receivedValue.Set(v));
                testResults.received.ReadingWith("receivedValue", testResults.receivedValue.Get);
                testResults.received.WritingWith<int>("receivedFailedValue", v => testResults.receivedFailedValue.Set(v));
                testResults.received.ReadingWith("receivedFailedValue", testResults.receivedFailedValue.Get);
                testResults.received.WritingWith<bool>("exceptionThrown", e => testResults.exceptionThrown.Set(e));
                testResults.received.ReadingWith("exceptionThrown", testResults.exceptionThrown.Get);
                return testResults;
            }

            public void SetGreeting(string greeting, bool isFailed = false) => received.WriteUsing(isFailed ? "receivedFailedGreeting" : "receivedGreeting", greeting);

            public void SetValue(int value, bool isFailed = false) => received.WriteUsing("receivedValue", value);

            public string GetGreeting(bool isFailed = false) => received.ReadFrom<string>(isFailed ? "receivedFailedGreeting" : "receivedGreeting");

            public int GetValue(bool isFailed = false) => received.ReadFrom<int>("receivedValue");

            public bool GetExceptionThrown() => received.ReadFrom<bool>("exceptionThrown");
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
        void CompletesNotSupportedForVoidReturnType();
    }
}