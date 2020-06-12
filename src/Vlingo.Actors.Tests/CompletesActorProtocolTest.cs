// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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

            uc.GetHello().AndThenConsume(hello => testResults.SetGreeting(hello.Greeting));
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
                .AndThen(hello => new Hello(Prefix + hello.Greeting))
                .AndThenConsume(hello => greetingsTestResults.SetGreeting(hello.Greeting));

            Assert.NotEqual(Hello, greetingsTestResults.GetGreeting());
            Assert.NotEqual(Hello, helloCompletes.Outcome.Greeting);
            Assert.Equal(Prefix + Hello, greetingsTestResults.GetGreeting());
            Assert.Equal(Prefix + Hello, helloCompletes.Outcome.Greeting);

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
                .AndThenConsume(TimeSpan.FromMilliseconds(1), new Hello(HelloNot), hello => greetingsTestResults.SetGreeting(hello.Greeting))
                .Otherwise<Hello>(failedHello =>
                {
                    greetingsTestResults.SetGreeting(failedHello.Greeting, true);
                    return failedHello;
                });
            
            Assert.NotEqual(Hello, greetingsTestResults.GetGreeting(true));
            Assert.Equal(HelloNot, helloCompletes.Outcome.Greeting);

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
                Thread.Sleep(150);
                oneCompletes.With(1); 
            });
            thread.Start();

            Assert.NotEqual(1, valueTestResults.GetValue(true));
            Assert.Equal(0, valueTestResults.GetValue(true));
            Assert.Equal(0, oneCompletes.Outcome);
        }

        private class UsesCompletesActor : Actor, IUsesCompletes
        {
            private readonly TestResults _results;

            public UsesCompletesActor(TestResults results) => _results = results;

            public void CompletesNotSupportedForVoidReturnType()
            {
                try
                {
                    Completes().With("Must throw exception");
                    _results.Received.WriteUsing("exceptionThrown", false);
                }
                catch (Exception)
                {
                    _results.Received.WriteUsing("exceptionThrown", true);
                }
            }

            public ICompletes<Hello> GetHello() => Completes().With(new Hello(Hello));

            public ICompletes<int> GetOne() => Completes().With(1);
        }

        private class UsesCompletesCausesTimeoutActor : Actor, IUsesCompletes
        {
            private readonly TestResults _results;

            public UsesCompletesCausesTimeoutActor(TestResults results) => _results = results;

            public void CompletesNotSupportedForVoidReturnType()
            {
                try
                {
                    Completes().With("Must throw exception");
                    _results.Received.WriteUsing("exceptionThrown", false);
                }
                catch
                {
                    _results.Received.WriteUsing("exceptionThrown", true);
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
            private readonly AtomicReference<string> _receivedGreeting = new AtomicReference<string>(null);
            private readonly AtomicReference<string> _receivedFailedGreeting = new AtomicReference<string>(null);
            private readonly AtomicInteger _receivedValue = new AtomicInteger(0);
            private readonly AtomicInteger _receivedFailedValue = new AtomicInteger(0);
            internal readonly AccessSafely Received;
            private readonly AtomicBoolean _exceptionThrown = new AtomicBoolean(false);

            public TestResults(AccessSafely received) => Received = received;

            public static TestResults AfterCompleting(int times)
            {
                var testResults = new TestResults(AccessSafely.AfterCompleting(times));
                testResults.Received.WritingWith<string>("receivedGreeting", s => testResults._receivedGreeting.Set(s));
                testResults.Received.ReadingWith("receivedGreeting", testResults._receivedGreeting.Get);
                testResults.Received.WritingWith<string>("receivedFailedGreeting", s => testResults._receivedFailedGreeting.Set(s));
                testResults.Received.ReadingWith("receivedFailedGreeting", testResults._receivedFailedGreeting.Get);
                testResults.Received.WritingWith<int>("receivedValue", v => testResults._receivedValue.Set(v));
                testResults.Received.ReadingWith("receivedValue", testResults._receivedValue.Get);
                testResults.Received.WritingWith<int>("receivedFailedValue", v => testResults._receivedFailedValue.Set(v));
                testResults.Received.ReadingWith("receivedFailedValue", testResults._receivedFailedValue.Get);
                testResults.Received.WritingWith<bool>("exceptionThrown", e => testResults._exceptionThrown.Set(e));
                testResults.Received.ReadingWith("exceptionThrown", testResults._exceptionThrown.Get);
                return testResults;
            }

            public void SetGreeting(string greeting, bool isFailed = false) => Received.WriteUsing(isFailed ? "receivedFailedGreeting" : "receivedGreeting", greeting);

            public void SetValue(int value, bool isFailed = false) => Received.WriteUsing(isFailed ? "receivedFailedValue" : "receivedValue", value);

            public string GetGreeting(bool isFailed = false) => Received.ReadFrom<string>(isFailed ? "receivedFailedGreeting" : "receivedGreeting");

            public int GetValue(bool isFailed = false) => Received.ReadFrom<int>(isFailed ? "receivedFailedValue" : "receivedValue");

            public bool GetExceptionThrown() => Received.ReadFrom<bool>("exceptionThrown");
        }
    }

    public class Hello
    {
        public readonly string Greeting;

        public Hello(string greeting) => Greeting = greeting;

        public override string ToString() => $"Hello[{Greeting}]";
    }

    public interface IUsesCompletes
    {
        ICompletes<Hello> GetHello();
        ICompletes<int> GetOne();
        void CompletesNotSupportedForVoidReturnType();
    }
}