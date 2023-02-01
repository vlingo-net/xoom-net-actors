// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors.Tests.Supervision;
using Xunit;
using SuspendedSenderSupervisorResults = Vlingo.Xoom.Actors.Tests.Supervision.SuspendedSenderSupervisorActor.SuspendedSenderSupervisorResults;

namespace Vlingo.Xoom.Actors.Tests
{
    public class ActorSuspendResumeTest : ActorsTest
    {
        [Fact]
        public void TestSuspendResume()
        {
            var testResults = new SuspendedSenderSupervisorResults();

            var failureControlSender = World.ActorFor<IFailureControlSender>(
                Definition.Has<SuspendedSenderSupervisorActor>(Definition.Parameters(testResults), "suspended-sender-supervisor"));

            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults),
                    SuspendedSenderSupervisorActor.Instance.Value,
                    "queueArray",
                    "failure"));
            var times = 25;
            var failureAccess = failureControlTestResults.AfterCompleting(times);
            var supervisorAccess = testResults.AfterCompleting(1);
            failureControlSender.SendUsing(failure, times);

            failure.FailNow();

            Assert.Equal(1, supervisorAccess.ReadFromExpecting("informedCount", 1));
            Assert.Equal(times, failureAccess.ReadFromExpecting("afterFailureCountCount", times));
        }
    }
}
