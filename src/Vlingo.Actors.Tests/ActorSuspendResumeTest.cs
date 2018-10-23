// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Actors.Tests.Supervision;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ActorSuspendResumeTest : ActorsTest
    {
        [Fact]
        public void TestSuspendResume()
        {
            var supervisor = World.ActorFor<IFailureControlSender>(
                Definition.Has<SuspendedSenderSupervisorActor>(Definition.NoParameters, "suspended-sender-supervisor"));
            var failureControlTestResults = new FailureControlActor.FailureControlTestResults();
            var failure = World.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(failureControlTestResults),
                    SuspendedSenderSupervisorActor.Instance.Value,
                    "queueArray",
                    "failure"));
            var times = 25;
            failureControlTestResults.UntilFailNow = Until(1);
            SuspendedSenderSupervisorActor.Instance.Value.UntilInformed = Until(1);
            failureControlTestResults.UntilFailureCount = Until(times - 1);
            supervisor.SendUsing(failure, times);

            failure.FailNow();
            failureControlTestResults.UntilFailNow.Completes();
            SuspendedSenderSupervisorActor.Instance.Value.UntilInformed.Completes();
            failureControlTestResults.UntilFailureCount.Completes();

            Assert.Equal(1, SuspendedSenderSupervisorActor.Instance.Value.InformedCount.Get());
            Assert.True(failureControlTestResults.AfterFailureCount.Get() >= times - 1);
        }
    }
}
