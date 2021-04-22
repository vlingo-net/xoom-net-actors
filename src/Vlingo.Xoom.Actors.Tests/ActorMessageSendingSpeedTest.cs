// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ActorMessageSendingSpeedTest
    {
        private const int Max = 100_000_000;

        private static DateTime _startTime;
        private static DateTime _endTime;

        [Fact]
        public void Test100millionSendsOnQueueMailbox()
        {
            // uncomment to run (too slow for build testing)
            // RunWith("queueMailbox");
        }

        [Fact]
        public void Test100millionSendsOnSharedRingBufferMailbox()
        {
            // uncomment to run (too slow for build testing)
            // RunWith("ringMailbox");
        }

        [Fact]
        public void Test100millionSendsOnArrayQueueMailbox()
        {
            // uncomment to run (too slow for build testing)
            // RunWith("arrayQueueMailbox");
        }

        protected void RunWith(string mailboxType)
        {
            var world = World.Start($"{GetType().Name}-world");

            var actor = world.ActorFor<ISingleOperation>(
                Definition.Has<SingleOperationActor>(
                    Definition.NoParameters,
                    mailboxType,
                    "single-op"));

            Console.WriteLine("======================================");
            Console.WriteLine("WARM UP: STARTING FOR MAILBOX TYPE: " + mailboxType);
            // warm up
            _endTime = DateTime.MinValue;
            SingleOperationActor.Instance.TotalValue = 0;
            for (int idx = 1; idx <= Max; ++idx)
            {
                actor.Keep(idx);
            }
            Console.WriteLine("WARM UP: SENT ALL, WAITING FOR COMPLETION");

            while (_endTime == DateTime.MinValue)
            {
                Thread.Sleep(100);
            }

            //======================================

            Console.WriteLine("SPEED TEST: START FOR MAILBOX TYPE: " + mailboxType);

            // speed test
            _endTime = DateTime.MinValue;
            SingleOperationActor.Instance.TotalValue = 0;
            _startTime = DateTime.UtcNow;
            for (int idx = 1; idx <= Max; ++idx)
            {
                actor.Keep(idx);
            }

            while (_endTime == DateTime.MinValue)
            {
                Thread.Sleep(500);
            }

            var totalTime = _endTime - _startTime;
            var totalSeconds = totalTime.TotalSeconds;

            Console.WriteLine("SPEED TEST: ENDED FOR MAILBOX TYPE: " + mailboxType);
            Console.WriteLine("          TOTAL TIME: " + totalTime);
            Console.WriteLine(" MESSAGES PER SECOND: " + (Max / totalSeconds));
        }

        

        private class SingleOperationActor : Actor, ISingleOperation
        {
            public int TotalValue;
            internal static SingleOperationActor Instance;

            public SingleOperationActor() => Instance = this;

            public void Keep(int value)
            {
                TotalValue = value;
                if (value == Max)
                {
                    _endTime = DateTime.UtcNow;
                }
            }
        }
    }
    public interface ISingleOperation
    {
        void Keep(int value);
    }
}
