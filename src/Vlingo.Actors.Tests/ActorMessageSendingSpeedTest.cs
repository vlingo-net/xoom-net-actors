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
    public class ActorMessageSendingSpeedTest
    {
        private const int Max = 100_000_000;

        private static DateTime StartTime;
        private static DateTime EndTime;

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
            var world = World.Start("speed-test");

            var actor = world.ActorFor<ISingleOperation>(
                Definition.Has<SingleOperationActor>(
                    Definition.NoParameters,
                    mailboxType,
                    "single-op"));

            Console.WriteLine("======================================");
            Console.WriteLine("WARM UP: STARTING FOR MAILBOX TYPE: " + mailboxType);
            // warm up
            EndTime = DateTime.MinValue;
            SingleOperationActor.instance.totalValue = 0;
            for (int idx = 1; idx <= Max; ++idx)
            {
                actor.Keep(idx);
            }
            Console.WriteLine("WARM UP: SENT ALL, WAITING FOR COMPLETION");

            while (EndTime == DateTime.MinValue)
            {
                Thread.Sleep(100);
            }

            //======================================

            Console.WriteLine("SPEED TEST: START FOR MAILBOX TYPE: " + mailboxType);

            // speed test
            EndTime = DateTime.MinValue;
            SingleOperationActor.instance.totalValue = 0;
            StartTime = DateTime.UtcNow;
            for (int idx = 1; idx <= Max; ++idx)
            {
                actor.Keep(idx);
            }

            while (EndTime == DateTime.MinValue)
            {
                Thread.Sleep(500);
            }

            var totalTime = EndTime - StartTime;
            var totalSeconds = totalTime.TotalSeconds;

            Console.WriteLine("SPEED TEST: ENDED FOR MAILBOX TYPE: " + mailboxType);
            Console.WriteLine("          TOTAL TIME: " + totalTime);
            Console.WriteLine(" MESSAGES PER SECOND: " + (Max / totalSeconds));
        }

        

        private class SingleOperationActor : Actor, ISingleOperation
        {
            public int totalValue;
            internal static SingleOperationActor instance;

            public SingleOperationActor()
            {
                instance = this;
            }

            public void Keep(int value)
            {
                totalValue = value;
                if (value == Max)
                {
                    EndTime = DateTime.UtcNow;
                }
            }
        }
    }
    public interface ISingleOperation
    {
        void Keep(int value);
    }
}
