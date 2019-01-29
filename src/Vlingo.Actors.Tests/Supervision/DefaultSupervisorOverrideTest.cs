// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Xunit;

namespace Vlingo.Actors.Tests.Supervision
{
    public class DefaultSupervisorOverrideTest : ActorsTest
    {
        [Fact]
        public void TestOverride()
        {
            var testResults = new FailureControlActor.FailureControlTestResults();
            var failure = TestWorld.ActorFor<IFailureControl>(
                Definition.Has<FailureControlActor>(
                    Definition.Parameters(testResults), "failure-for-stop"));
            testResults.UntilFailNow = Until(20);
            testResults.UntilAfterFail = Until(20);
            for(var idx = 0; idx < 20; ++idx)
            {
                testResults.UntilBeforeResume = Until(1);
                failure.Actor.FailNow();
                testResults.UntilBeforeResume.Completes();
                failure.Actor.AfterFailure();
            }
            testResults.UntilFailNow.Completes();
            testResults.UntilAfterFail.Completes();

            testResults.UntilFailNow = Until(20);
            testResults.UntilAfterFail = Until(20);
            for (var idx = 0; idx < 20; ++idx)
            {
                testResults.UntilBeforeResume = Until(1);
                failure.Actor.FailNow();
                testResults.UntilBeforeResume.Completes();
                failure.Actor.AfterFailure();
            }
            testResults.UntilFailNow.Completes();
            testResults.UntilAfterFail.Completes();

            Assert.False(failure.ActorInside.IsStopped);
            Assert.Equal(40, testResults.FailNowCount.Get());
            Assert.Equal(40, testResults.AfterFailureCount.Get());
        }
    }
}
