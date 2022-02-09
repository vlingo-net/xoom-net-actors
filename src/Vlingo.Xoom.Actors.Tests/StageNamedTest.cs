// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class StageNamedTest : ActorsTest
    {
        [Fact]
        public void TestWorldStageNamedOnce()
        {
            var defaultStage = World.Stage;
            var uniqueName = Guid.NewGuid().ToString("N");
            var uniqueStage = World.StageNamed(uniqueName);

            Assert.NotSame(defaultStage, uniqueStage);
            Assert.Equal(uniqueName, uniqueStage.Name);
            Assert.Same(uniqueStage, World.StageNamed(uniqueName));
        }

        [Fact]
        public void TestActorStageNamed()
        {
            var defaultStage = TestWorld.Stage;
            var query = TestWorld.ActorFor<IStageNameQuery>(Definition.Has<StageNamedWithResultActor>(Definition.NoParameters));
            var result = TestWorld.ActorFor<IStageNameQueryResult>(Definition.Has<StageNamedWithResultActor>(Definition.NoParameters));
            var uniqueName = Guid.NewGuid().ToString("N");

            query.Actor.StageNamed(uniqueName, result.Actor);
            var stageHolder = result.ViewTestState().ValueOf<Stage>("stageHolder");

            Assert.Equal(1, TestWorld.AllMessagesFor(query.Address).Count);
            Assert.Equal(1, TestWorld.AllMessagesFor(result.Address).Count);
            Assert.NotSame(defaultStage, stageHolder);
            Assert.Same(stageHolder, TestWorld.StageNamed(uniqueName));
        }

        private class StageNamedWithResultActor : Actor, IStageNameQuery, IStageNameQueryResult
        {
            private Stage stageHolder;
            public void StageNamed(string name, IStageNameQueryResult result) => result.StageWithNameResult(StageNamed(name), name);
            public void StageWithNameResult(Stage stage, string name) => stageHolder = stage;
            public override TestState ViewTestState() => new TestState().PutValue("stageHolder", stageHolder);
        }
    }

    public interface IStageNameQueryResult
    {
        void StageWithNameResult(Stage stage, string name);
    }

    public interface IStageNameQuery
    {
        void StageNamed(string name, IStageNameQueryResult result);
    }
}
