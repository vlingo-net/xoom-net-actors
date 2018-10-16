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
    public class OutcomeInterestTest : ActorsTest
    {
        [Fact]
        public void TestOutcomeInterestSuccess()
        {
            var test = World.ActorFor<ITestsOutcomeAware>(Definition.Has<DoSomethingOutcomeAwareActor>(Definition.NoParameters));
            test.DoSomething();

            Thread.Sleep(500);
        }

        private class DoSomethingOutcomeAwareActor : Actor, ITestsOutcomeAware, IOutcomeAware<string, int>
        {
            private ITestsOutcomeInterest testsInterest;

            public DoSomethingOutcomeAwareActor()
            {
                testsInterest = Stage.ActorFor<ITestsOutcomeInterest>(
                    Definition.Has<DoSomethingWithOutcomeInterestActor>(
                        Definition.NoParameters));
            }

            public void DoSomething()
            {
                var interest = SelfAsOutcomeInterest<string, int>(1);
                testsInterest.DoSomethingWith("something", interest);
            }

            public void FailureOutcome(Outcome<string> outcome, int reference)
            {
                Console.WriteLine($"FAILURE: outcome={outcome} reference={reference}");
            }

            public void SuccessfulOutcome(Outcome<string> outcome, int reference)
            {
                Console.WriteLine($"SUCCESS: outcome={outcome.Value} reference={reference}");
            }
        }

        private class DoSomethingWithOutcomeInterestActor : Actor, ITestsOutcomeInterest
        {
            public void DoSomethingWith(string text, IOutcomeInterest<string> interest)
            {
                interest.SuccessfulOutcome(new SuccessfulOutcome<string>($"{text}-something-else"));
            }
        }
    }

    public interface ITestsOutcomeAware
    {
        void DoSomething();
    }

    public interface ITestsOutcomeInterest
    {
        void DoSomethingWith(string text, IOutcomeInterest<string> interest);
    }
}
