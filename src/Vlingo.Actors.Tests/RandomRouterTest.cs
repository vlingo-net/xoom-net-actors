// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class RandomRouterTest
    {
        [Fact]
        public void TestThatItRoutes()
        {
            var world = World.StartWithDefault("RandomRouterTest");
            const int poolSize = 4, messagesToSend = 40;
            var until = TestUntil.Happenings(messagesToSend);

            var orderRouter = world.ActorFor<IOrderRandomRouter>(Definition.Has<OrderRouterActor>(Definition.Parameters(poolSize, until)));
            var random = new Random();

            for (int i = 0; i < messagesToSend; i++)
            {
                orderRouter.RouteOrder(new RandomRouterOrder());
            }

            until.Completes();
        }

        private class OrderRouterWorker : Actor, IOrderRandomRouter
        {
            private readonly TestUntil testUntil;

            public OrderRouterWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void RouteOrder(RandomRouterOrder order)
            {
                Logger.Log($"{ToString()} is routing {order}");
                testUntil.Happened();
            }
        }

        private class OrderRouterActor : Router, IOrderRandomRouter
        {
            public OrderRouterActor(int poolSize, TestUntil testUntil)
                : base(
                      new RouterSpecification(poolSize, Definition.Has<OrderRouterWorker>(Definition.Parameters(testUntil)), typeof(IOrderRandomRouter)),
                      new RandomRoutingStrategy())
            {
            }

            public void RouteOrder(RandomRouterOrder order)
            {
                ComputeRouting(order)
                    .RouteesAs<IOrderRandomRouter>()
                    .ToList()
                    .ForEach(x => x.RouteOrder(order));
            }
        }

        
    }

    public class RandomRouterOrder
    {
        public RandomRouterOrder()
        {
            OrderId = Guid.NewGuid().ToString();
        }

        public string OrderId { get; }

        public override string ToString()
        {
            return $"Order[orderId={OrderId}]";
        }
    }

    public interface IOrderRandomRouter
    {
        void RouteOrder(RandomRouterOrder order);
    }

    public class OrderRandomRouter__Proxy : IOrderRandomRouter
    {
        private const string RouteOrderRepresentation1 = "RouteOrder(RandomRouterOrder)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public OrderRandomRouter__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void RouteOrder(RandomRouterOrder order)
        {
            if (!actor.IsStopped)
            {
                Action<IOrderRandomRouter> consumer = x => x.RouteOrder(order);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, RouteOrderRepresentation1);
                }
                else
                {
                    mailbox.Send(new LocalMessage<IOrderRandomRouter>(actor, consumer, RouteOrderRepresentation1));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RouteOrderRepresentation1));
            }
        }

    }
}
