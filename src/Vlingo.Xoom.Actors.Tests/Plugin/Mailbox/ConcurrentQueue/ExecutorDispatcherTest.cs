// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vlingo.Xoom.Actors.Plugin.Mailbox.ConcurrentQueue;
using Vlingo.Xoom.Actors.TestKit;
using Vlingo.Xoom.Common;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests.Plugin.Mailbox.ConcurrentQueue
{
    public class ExecutorDispatcherTest : ActorsTest
    {
        private const int Total = 10_000;

        private readonly IDispatcher _dispatcher;

        public ExecutorDispatcherTest()
        {
            _dispatcher = new ExecutorDispatcher(1, 0, 1f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _dispatcher.Close();
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
                _dispatcher.Execute(mailbox);
            }

            _dispatcher.Close();

            Action<ICountTaker> consumer2 = consumerActor => consumerActor.Take(10);
            var message2 = new LocalMessage<ICountTaker>(actor, consumer2, "Take(int)");
            mailbox.Send(message2);
            _dispatcher.Execute(mailbox);

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
                _dispatcher.Execute(mailbox);
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
            Assert.False(_dispatcher.RequiresExecutionNotification);
        }
        
        [Fact]
        public void TestThatPoolSizeSet()
        {
            var dispatcher1 = new ExecutorDispatcher(1, 10, 0);
            var dispatcher2 = new ExecutorDispatcher(1, 8, 20.5f);
            var dispatcher3 = new ExecutorDispatcher(1, 5, 10.0f);

            Assert.Equal(10, dispatcher1.ConcurrencyCapacity);
            Assert.Equal(8, dispatcher2.ConcurrencyCapacity);
            Assert.Equal(5, dispatcher3.ConcurrencyCapacity);
        }

        private class TestMailbox : IMailbox
        {
            private readonly TestResults _testResults;
            private readonly ILogger _logger;
            private readonly ConcurrentQueue<IMessage> _queue;

            public TestMailbox(TestResults testResults, ILogger logger)
            {
                _testResults = testResults;
                _logger = logger;
                _queue = new ConcurrentQueue<IMessage>();
            }

            public TaskScheduler TaskScheduler { get; }

            public bool IsClosed => false;

            public bool IsDelivering => false;
            public int ConcurrencyCapacity => 1;

            public bool IsPreallocated => false;

            public int PendingMessages => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

            public bool IsSuspendedFor(string name) => IsSuspended;

            public bool IsSuspended => false;

            public void Close() { }

            public void Resume(string name) => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

            public IMessage Receive()
            {
                if (_queue.TryDequeue(out var message))
                {
                    return message;
                }

                return null;
            }

            public void Run()
            {
                var message = Receive();
                if (_testResults.ShouldLog)
                {
                    _logger.Debug($"TestMailBox: Run: received: {message}");
                }

                if (message != null)
                {
                    message.Deliver();
                    if (_testResults.ShouldLog)
                    {
                        _logger.Debug($"TestMailBox: Run: adding: {_testResults.GetHighest()}");
                    }
                }
            }

            public void Send(IMessage message) => _queue.Enqueue(message);

            public void Send<T>(Actor actor, Action<T> consumer, ICompletes completes, string representation)
                => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");
            
            public void Send(Actor actor, Type protocol, LambdaExpression consumer, ICompletes completes, string representation) => 
                throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");

            public void SuspendExceptFor(string name, params Type[] overrides)
                => throw new NotSupportedException("ExecutorDispatcherTest does not support this operation");
        }

        private class CountTakerActor : Actor, ICountTaker
        {
            private readonly TestResults _testResults;
            private readonly ILogger _logger;

            public CountTakerActor(TestResults testResults, ILogger logger)
            {
                _testResults = testResults;
                _logger = logger;
            }

            public void Take(int count) => _testResults.Take(count, _logger);
        }

        private class TestResults
        {
            private readonly AccessSafely _safely;
            internal readonly bool ShouldLog;

            public TestResults(int happenings, bool shouldLog)
            {
                ShouldLog = shouldLog;

                var counts = new List<int>();
                var highest = new AtomicInteger(0);

                _safely = AccessSafely
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

            public void Take(int count, ILogger logger) => _safely.WriteUsing("results", count, logger);

            public List<int> GetCounts() => _safely.ReadFrom<List<int>>("results");

            public int GetHighest() => _safely.ReadFrom<int>("highest");
        }
    }
}
