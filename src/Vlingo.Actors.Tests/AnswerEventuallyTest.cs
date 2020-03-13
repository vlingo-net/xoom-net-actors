// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.TestKit;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class AnswerEventuallyTest : IDisposable
    {
        private AtomicInteger _value = new AtomicInteger(0);
        private IAnswerGiver _answerGiver;
        private readonly World _world;
        
        [Fact]
        public void TestThatActorAnswersEventually()
        {
            var access = AccessSafely.AfterCompleting(1);
            access.WritingWith<int>("answer", answer => _value.Set(answer));
            access.ReadingWith("answer", () => _value.Get());

            _answerGiver.Calculate("10", 5).AndThenConsume(answer => access.WriteUsing("answer", answer));

            var answer = access.ReadFrom<int>("answer");

            Assert.Equal(50, answer);
        }
        
        public AnswerEventuallyTest()
        {
            _world = World.StartWithDefaults("test-answer-eventually");
            _answerGiver = _world.ActorFor<IAnswerGiver>(typeof(AnswerGiverActor));
        }
        
        public void Dispose()
        {
            _world.Terminate();
        }
    }
    
    public interface IAnswerGiver
    {
        ICompletes<int> Calculate(string text, int multiplier);
    }
    
    public class AnswerGiverActor : Actor, IAnswerGiver
    {
        private ITextToInteger _textToInteger;

        public override void Start() => _textToInteger = ChildActorFor<ITextToInteger>(Definition.Has<TextToIntegerActor>(Definition.NoParameters));

        public ICompletes<int> Calculate(string text, int multiplier)
        {
            return AnswerFrom(_textToInteger.ConvertFrom(text).AndThen(number => number * multiplier));
        }
    }

    public interface ITextToInteger
    {
        ICompletes<int> ConvertFrom(string text);
    }

    public class TextToIntegerActor : Actor, ITextToInteger
    {
        public ICompletes<int> ConvertFrom(string text) => Completes().With(int.Parse(text));
    }
}