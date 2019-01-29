// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
    public class SmallestMailboxRouterTest
    {
        [Fact]
        public void TestThatItRoutes()
        {
            var world = World.StartWithDefault("SmallestMailboxRouterTest");
            const int poolSize = 4, messagesToSend = 40;
            var until = TestUntil.Happenings(messagesToSend);

            var orderRouter = world.ActorFor<IOrderSmallestRouter>(Definition.Has<OrderRouterActor>(Definition.Parameters(poolSize, until)));
            var random = new Random();

            for (int i = 0; i < messagesToSend; i++)
            {
                orderRouter.RouteOrder(new SmallestRouterOrder());
            }

            until.Completes();
        }

        private class OrderRouterWorker : Actor, IOrderSmallestRouter
        {
            private readonly TestUntil testUntil;

            public OrderRouterWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void RouteOrder(SmallestRouterOrder order)
            {
                Logger.Log($"{ToString()} is routing {order}");
                testUntil.Happened();
            }
        }

        private class OrderRouterActor : Router, IOrderSmallestRouter
        {
            public OrderRouterActor(int poolSize, TestUntil testUntil)
                : base(
                      new RouterSpecification(poolSize, Definition.Has<OrderRouterWorker>(Definition.Parameters(testUntil)), typeof(IOrderSmallestRouter)),
                      new SmallestMailboxRoutingStrategy())
            {
            }

            public void RouteOrder(SmallestRouterOrder order)
            {
                ComputeRouting(order)
                    .RouteesAs<IOrderSmallestRouter>()
                    .ToList()
                    .ForEach(x => x.RouteOrder(order));
            }
        }
    }

    public class SmallestRouterOrder
    {
        public SmallestRouterOrder()
        {
            OrderId = Guid.NewGuid().ToString();
        }

        public string OrderId { get; }

        public override string ToString()
        {
            return $"Order[orderId={OrderId}]";
        }
    }

    public interface IOrderSmallestRouter
    {
        void RouteOrder(SmallestRouterOrder order);
    }
}
