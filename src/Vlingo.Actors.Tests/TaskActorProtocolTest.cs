// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;
using Vlingo.Common;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class TaskActorProtocolTest : ActorsTest
    {
        [Fact(Skip = "Mailbox suspension is not implemented yet")]
        public async Task TestThatMailboxDoesntDeliverWhileAwaiting()
        {
            var uc = World.ActorFor<IUsesTask>(typeof(UsesTaskActor));
            var thread = new Thread(() =>
            {
                Thread.Sleep(1000);
                uc.Change(10);
            });
            thread.Start();
            
            var one = await uc.GetOne();
            Assert.Equal(1, one);
        }
        
        [Fact]
        public async Task TestThatTaskLikeCompletesCanBeAwaited()
        {
            var uc = World.ActorFor<IUsesTask>(typeof(UsesTaskActor));
            var two = await uc.GetTwoAsync();
            Assert.Equal(2, two);
        }
    }
    
    public class UsesTaskActor : Actor, IUsesTask
    {
        private int _one = 1;
        
        public async Task<int> GetOne()
        {
            await Task.Delay(5000);
            return _one;
        }

        public async ICompletes<int> GetTwoAsync()
        {
            await Task.Delay(5000);
            return await Completes().With(2);
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