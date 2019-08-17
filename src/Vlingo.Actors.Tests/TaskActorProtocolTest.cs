// Copyright © 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
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
    
    public class UsesTask__Proxy : Vlingo.Actors.Tests.IUsesTask
    {
        private const string ChangeRepresentation1 = "Change(int)";
        private const string GetOneRepresentation2 = "GetOne()";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public UsesTask__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void Change(int newOne)
        {
            if(!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.Tests.IUsesTask> consumer = __ => __.Change(newOne);
                if(this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, consumer, null, ChangeRepresentation1);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Actors.Tests.IUsesTask>(this.actor, consumer, ChangeRepresentation1));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ChangeRepresentation1));
            }
        }
        public System.Threading.Tasks.Task<int> GetOne()
        {
            if(!this.actor.IsStopped)
            {
                var tcs = new TaskCompletionSource<int>();
                Action<Vlingo.Actors.Tests.IUsesTask> consumer = async __ =>
                {
                    try
                    {
                        tcs.SetResult(await __.GetOne());
                    }
                    catch (AggregateException e)
                    {
                        tcs.SetException(e);
                    }
                };
                if(this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, consumer, null, GetOneRepresentation2);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Actors.Tests.IUsesTask>(this.actor, consumer, GetOneRepresentation2));
                }

                return tcs.Task;
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, GetOneRepresentation2));
            }
            return null;
        }
    }
}