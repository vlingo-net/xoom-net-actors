// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Vlingo.Xoom.Actors.TestKit;
using Xunit;

namespace Vlingo.Xoom.Actors.Tests
{
    public class TaskExtensionsTest : IDisposable
    {
        private readonly TaskCompletionSource<string> _taskCompletionSource;
        private readonly Task<string> _task;
        private readonly World _world;
        
        [Fact]
        public void TestThatSendTaskResultAsMessage()
        {
            var access = AccessSafely.AfterCompleting(1);
            var actor = _world.Stage.ActorFor<IGreetings>(() => new TestActor(access));
            _task.AndThenTo(actor.Greet);
            _taskCompletionSource.SetResult("Hello");
            Assert.Equal("Hello", access.ReadFrom<string>("greeting"));
        }
        
        [Fact]
        public void TestThatSendTaskExceptionAsMessage()
        {
            var access = AccessSafely.AfterCompleting(1);
            var actor = _world.Stage.ActorFor<IGreetings>(() => new TestActor(access));
            _task.AndThenTo(actor.Greet, actor.GreetFault);
            _taskCompletionSource.SetException(new Exception("No greet message"));
            Assert.Equal("No greet message", access.ReadFrom<Exception>("greetException").InnerException.Message);
        }
        
        public TaskExtensionsTest()
        {
           _taskCompletionSource = new TaskCompletionSource<string>();
           _task = _taskCompletionSource.Task;
           _world = World.StartWithDefaults("task-extension-actor");
        }
        
        public interface IGreetings
        {
            void GreetFault(Exception exception);
            void Greet(string message);
        }
        
        public class TestActor : Actor, IGreetings
        {
            private readonly AccessSafely _access;
            private string _greetingMessage;
            private Exception _greetException;

            public void GreetFault(Exception exception) => _access.WriteUsing("greetException", exception);
            
            public void Greet(string message) => _access.WriteUsing("greeting", message);
            
            public TestActor(AccessSafely access)
            {
                _access = access
                    .WritingWith<string>("greeting", m => _greetingMessage = m)
                    .ReadingWith("greeting", () => _greetingMessage)
                    .WritingWith<Exception>("greetException", e => _greetException = e)
                    .ReadingWith("greetException", () => _greetException);
            }
        }

        public void Dispose() => _world.Terminate();
    }
}