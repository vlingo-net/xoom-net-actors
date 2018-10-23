// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests.Plugin.Mailbox.ConcurrentQueue
{
    public class ExecutorDispatcherTest : ActorsTest
    {
        private const int Total = 10_000;

        private readonly IDispatcher dispatcher;

        public ExecutorDispatcherTest()
        {
            dispatcher = new ExecutorDispatcher(1, 1f);
        }

        public override void Dispose()
        {
            base.Dispose();
            dispatcher.Close();
        }

        [Fact]
        public void TestClose()
        {
            var testResults = new TestResults();
            testResults.Log.Set(true);
            var mailbox = new TestMailbox(testResults);
            var actor = new CountTakerActor(testResults);
            testResults.Until = Until(3);

            for (var count = 0; count < 3; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
                dispatcher.Execute(mailbox);
            }
            testResults.Until.Completes();

            dispatcher.Close();

            Action<ICountTaker> consumer2 = consumerActor => consumerActor.Take(10);
            var message2 = new LocalMessage<ICountTaker>(actor, consumer2, "Take(int)");
            mailbox.Send(message2);
            dispatcher.Execute(mailbox);

            Assert.Equal(3, testResults.Counts.Count);
            for (var idx = 0; idx < testResults.Counts.Count; ++idx)
            {
                Assert.Contains(idx, testResults.Counts);
            }
        }

        [Fact]
        public void TestExecute()
        {
            var testResults = new TestResults();
            testResults.Log.Set(true);
            var mailbox = new TestMailbox(testResults);
            var actor = new CountTakerActor(testResults);
            testResults.Until = Until(Total);

            for (var count = 0; count < Total; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
                dispatcher.Execute(mailbox);
            }
            testResults.Until.Completes();

            for (var idx = 0; idx < testResults.Counts.Count; ++idx)
            {
                Assert.Contains(idx, testResults.Counts);
            }
        }

        [Fact]
        public void TestRequiresExecutionNotification()
        {
            Assert.False(dispatcher.RequiresExecutionNotification);
        }


        private class TestMailbox : IMailbox
        {
            private readonly TestResults testResults;
            private readonly ConcurrentQueue<IMessage> queue;

            public TestMailbox(TestResults testResults)
            {
                this.testResults = testResults;
                queue = new ConcurrentQueue<IMessage>();
            }

            public bool IsClosed => false;

            public bool IsDelivering => false;

            public void Close() { }

            public bool Delivering(bool flag) => flag;

            public IMessage Receive()
            {
                if (queue.TryDequeue(out var message))
                {
                    return message;
                }

                return null;
            }

            public void Run()
            {
                var message = Receive();
                if (testResults.Log.Get())
                {
                    Console.WriteLine($"TestMailBox: Run: received: {message}");
                }

                if (message != null)
                {
                    message.Deliver();
                    if (testResults.Log.Get())
                    {
                        Console.WriteLine($"TestMailBox: Run: adding: {testResults.Highest.Get()}");
                    }
                }
            }

            public void Send(IMessage message) => queue.Enqueue(message);
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults testResults;

            public CountTakerActor(TestResults testResults)
            {
                this.testResults = testResults;
            }

            public void Take(int count)
            {
                if (testResults.Log.Get())
                {
                    Console.WriteLine($"CountTakerActor: Take: count: {count}");
                }

                if (count > testResults.Highest.Get())
                {
                    if (testResults.Log.Get())
                    {
                        Console.WriteLine($"CountTakerActor: Run: received: {count} > {testResults.Highest.Get()}");
                    }

                    testResults.Highest.Set(count);
                }
                testResults.Counts.Add(testResults.Highest.Get());
                testResults.Until.Happened();
            }
        }

        private class TestResults
        {
            public AtomicBoolean Log = new AtomicBoolean(false);
            public ConcurrentBag<int> Counts = new ConcurrentBag<int>();
            public AtomicInteger Highest = new AtomicInteger(0);
            public TestUntil Until = TestUntil.Happenings(0);
        }
    }
}
