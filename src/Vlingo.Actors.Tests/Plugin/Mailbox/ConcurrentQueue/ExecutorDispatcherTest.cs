// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Common;
using Vlingo.Actors.TestKit;
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
            var testResults = new TestResults(3, false);
            var mailbox = new TestMailbox(testResults, World.DefaultLogger);
            var actor = new CountTakerActor(testResults, World.DefaultLogger);

            for (var count = 0; count < 3; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
                dispatcher.Execute(mailbox);
            }

            dispatcher.Close();

            Action<ICountTaker> consumer2 = consumerActor => consumerActor.Take(10);
            var message2 = new LocalMessage<ICountTaker>(actor, consumer2, "Take(int)");
            mailbox.Send(message2);
            dispatcher.Execute(mailbox);

            var counts = testResults.GetCounts();
            Assert.Equal(3, counts.Count);
            for (var idx = 0; idx < counts.Count; ++idx)
            {
                Assert.Contains(idx, counts);
            }
        }

        [Fact]
        public void TestExecute()
        {
            var testResults = new TestResults(Total, false);
            var mailbox = new TestMailbox(testResults, World.DefaultLogger);
            var actor = new CountTakerActor(testResults, World.DefaultLogger);

            for (var count = 0; count < Total; ++count)
            {
                var countParam = count;
                Action<ICountTaker> consumer = consumerActor => consumerActor.Take(countParam);
                var message = new LocalMessage<ICountTaker>(actor, consumer, "Take(int)");
                mailbox.Send(message);
                dispatcher.Execute(mailbox);
            }

            List<int> counts = testResults.GetCounts();
            Assert.Equal(Total, counts.Count);
            for (var idx = 0; idx < counts.Count; ++idx)
            {
                Assert.Contains(idx, counts);
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
            private readonly ILogger logger;
            private readonly ConcurrentQueue<IMessage> queue;

            public TestMailbox(TestResults testResults, ILogger logger)
            {
                this.testResults = testResults;
                this.logger = logger;
                queue = new ConcurrentQueue<IMessage>();
            }
            
            public TaskScheduler TaskScheduler { get; }

            public bool IsClosed => false;

            public bool IsDelivering => false;

            public bool IsPreallocated => false;

            public int PendingMessages => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

            public bool IsSuspended => false;

            public void Close() { }

            public void Resume(string name) => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

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
                if (testResults.shouldLog)
                {
                    logger.Debug($"TestMailBox: Run: received: {message}");
                }

                if (message != null)
                {
                    message.Deliver();
                    if (testResults.shouldLog)
                    {
                        logger.Debug($"TestMailBox: Run: adding: {testResults.GetHighest()}");
                    }
                }
            }

            public void Send(IMessage message) => queue.Enqueue(message);

            public void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
                => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

            public void SuspendExceptFor(string name, params Type[] overrides)
                => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults testResults;
            private readonly ILogger logger;

            public CountTakerActor(TestResults testResults, ILogger logger)
            {
                this.testResults = testResults;
                this.logger = logger;
            }

            public void Take(int count) => testResults.Take(count, logger);
        }

        private class TestResults
        {
            private readonly AccessSafely safely;
            internal readonly bool shouldLog;

            public TestResults(int happenings, bool shouldLog)
            {
                this.shouldLog = shouldLog;

                var counts = new List<int>();
                var highest = new AtomicInteger(0);

                safely = AccessSafely
                    .AfterCompleting(happenings)
                    .WritingWith<int, ILogger>("results", (count, logger) =>
                    {
                        if (shouldLog)
                        {
                            logger.Debug($"CountTakerActor: take: {count}");
                        }

                        if (count > highest.Get())
                        {
                            if (shouldLog)
                            {
                                logger.Debug($"CountTakerActor: take: {count} > {highest.Get()}");
                            }
                            highest.Set(count);
                        }

                        counts.Add(highest.Get());
                    })
                    .ReadingWith("results", () => counts)
                    .ReadingWith("highest", highest.Get);
            }

            public void Take(int count, ILogger logger) => safely.WriteUsing("results", count, logger);

            public List<int> GetCounts() => safely.ReadFrom<List<int>>("results");

            public int GetHighest() => safely.ReadFrom<int>("highest");
        }
    }
}
