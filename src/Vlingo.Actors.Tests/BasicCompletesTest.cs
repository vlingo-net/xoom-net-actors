// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class BasicCompletesTest
    {
        [Fact]
        public void TestCompletesWith()
        {
            var completes = new BasicCompletes<int>(5);

            Assert.Equal(5, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterSupplier()
        {
            var completes = new BasicCompletes<int>(0);
            completes.After(() => completes.Outcome * 2);

            completes.With(5);

            Assert.Equal(10, completes.Outcome);
        }

        [Fact]
        public void TestCompletesAfterConsumer()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            completes.After(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(5, andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThen()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            completes
                .After(() => completes.Outcome * 2)
                .AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestCompletesAfterAndThenMessageOut()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(0);
            var sender = new Sender(x => andThenValue = x);

            completes
                .After(() => completes.Outcome * 2)
                .AndThen(x => sender.Send(x));

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestOutcomeBeforeTimeout()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(new Scheduler());

            completes
                .After(() => completes.Outcome * 2, 1000)
                .AndThen(x => andThenValue = x);

            completes.With(5);

            Assert.Equal(10, andThenValue);
        }

        [Fact]
        public void TestTimeoutBeforeOutcome()
        {
            int andThenValue = 0;
            var completes = new BasicCompletes<int>(new Scheduler());

            completes
                .After(() => completes.Outcome * 2, 1, 0)
                .AndThen(x => andThenValue = x);

            Thread.Sleep(1000);
            completes.With(5);

            Assert.NotEqual(10, andThenValue);
            Assert.Equal(0, andThenValue);
        }

        private class Sender
        {
            private Action<int> callback;
            public Sender(Action<int> callback)
            {
                if(callback != null)
                {
                    this.callback = callback;
                }
            }

            internal void Send(int value)
            {
                callback(value);
            }
        }
    }
}
