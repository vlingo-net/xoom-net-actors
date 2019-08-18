// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class TaskActorProtocolTest : ActorsTest
    {
        [Fact]
        public async Task TestThatMailboxDoesntDeliverWhileAwaiting()
        {
            var uc = World.ActorFor<IUsesTask>(typeof(UsesTaskActor));
            var one = await uc.GetOne();
            uc.Change(10);
            Assert.Equal(1, one);
        }
    }
    
    public class UsesTaskActor : Actor, IUsesTask
    {
        private int one = 1;
        public async Task<int> GetOne()
        {
            await Task.Delay(5000);
            return one;
        }

        public void Change(int newOne)
        {
            one = newOne;
        }
    }

    public interface IUsesTask
    {
        void Change(int newOne);
        
        Task<int> GetOne();
    }
}