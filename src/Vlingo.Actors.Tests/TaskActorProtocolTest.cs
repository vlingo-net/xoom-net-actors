// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using System.Threading.Tasks;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class TaskActorProtocolTest : ActorsTest
    {
        [Fact]
        public async Task TestThatMailboxDoesntDeliverWhileAwaiting()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesTaskActor());
            var thread = new Thread(() =>
            {
                Thread.Sleep(100);
                uc.Change(10);
            });
            thread.Start();
            
            var one = await uc.GetOne();
            Assert.Equal(1, one);
        }
        
        [Fact]
        public async Task TestThatExceptionIsCorrectlyHandled()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesFailingTaskActor());

            try
            {
                await uc.GetOne();
            }
            catch (Exception e)
            {
                Assert.Equal("Cannot perform the operation", e.Message);
            }
        }
        
        [Fact]
        public async Task TestThatSchedulesLongRunningOperations()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesLongRunningTaskActor());
            var one = await uc.GetOne();
            Assert.Equal(1, one);
        }
        
        [Fact]
        public async Task TestThatSchedulesLongRunningOperationsAndSuspendMailbox()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesLongRunningTaskActor());
            var thread = new Thread(() =>
            {
                Thread.Sleep(100);
                uc.Change(10);
            });
            thread.Start();
            
            var one = await uc.GetOne();
            Assert.Equal(1, one);
        }
        
        [Fact]
        public async Task TestThatTaskLikeCompletesCanBeAwaited()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesTaskActor());
            var two = await uc.GetTwoAsync();
            Assert.Equal(2, two);
        }
        
        [Fact]
        public async Task TestThatMailboxDoesntDeliverForCompletesWhileAwaiting()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesTaskActor());
            var thread = new Thread(() =>
            {
                Thread.Sleep(5000);
                uc.Change(10);
            });
            thread.Start();
            
            var one = await uc.GetTwoAsync();
            Assert.Equal(2, one);
        }
        
        [Fact]
        public async Task TestThatSchedulesLongRunningOperationsForCompletesAndSuspendMailbox()
        {
            var uc = World.ActorFor<IUsesTask>(() => new UsesLongRunningTaskActor());
            var thread = new Thread(() =>
            {
                Thread.Sleep(100);
                uc.Change(10);
            });
            thread.Start();
            
            var one = await uc.GetTwoAsync();
            Assert.Equal(2, one);
        }
    }
    
    public class UsesTaskActor : Actor, IUsesTask
    {
        private int _one = 1;
        
        public async Task<int> GetOne()
        {
            await Task.Delay(1000);
            return _one;
        }

        public async ICompletes<int> GetTwoAsync()
        {
            await Task.Delay(1000);
            return await Completes().With(_one + 1);
        }

        public void Change(int newOne)
        {
            _one = newOne;
        }
    }
    
    public class UsesFailingTaskActor : Actor, IUsesTask
    {
        public async Task<int> GetOne()
        {
            await Task.Delay(100);
            throw new Exception("Cannot perform the operation");
        }

        public async ICompletes<int> GetTwoAsync()
        {
            await Task.Delay(100);
            throw new Exception("Cannot perform the operation");
        }

        public void Change(int newOne)
        {
        }
    }
    
    public class UsesLongRunningTaskActor : Actor, IUsesTask
    {
        private int _one = 1;
        
        public Task<int> GetOne()
        {
            return Task<int>.Factory.StartNew(() =>
            {
                Task.Delay(2000).Wait();
                return _one;
            }, TaskCreationOptions.LongRunning);
        }

        public async ICompletes<int> GetTwoAsync()
        {
            await Task.Delay(100);
            var result = await Task<int>.Factory.StartNew(() =>
            {
                Task.Delay(2000).Wait();
                return _one;
            }, TaskCreationOptions.LongRunning)
                .ContinueWith(t => t.Result + 1);

            return await Completes().With(result);
        }

        public void Change(int newOne)
        {
            _one = newOne;
        }
    }

    public interface IUsesTask
    {
        void Change(int newOne);
        
        Task<int> GetOne();

        ICompletes<int> GetTwoAsync();
    }
}