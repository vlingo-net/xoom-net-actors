// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class RoundRobinRouterTest
    {
        [Fact]
        public void TestThatItRoutes()
        {
            var world = World.StartWithDefault("RoundRobinRouterTest");
            const int poolSize = 4, messagesToSend = 8;
            var until = TestUntil.Happenings(messagesToSend);
            var orderRouter = world.ActorFor<IRoundRobinOrderRouter>(
                Definition.Has<OrderRouterActor>(Definition.Parameters(poolSize, until)));

            for(var i=0; i<messagesToSend; ++i)
            {
                orderRouter.RouteOrder(new RoundRobinOrder(i));
            }

            until.Completes();
        }

        private class OrderRouterWorker : Actor, IRoundRobinOrderRouter
        {
            private readonly TestUntil testUntil;

            public OrderRouterWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void RouteOrder(RoundRobinOrder order)
            {
                Logger.Log($"{ToString()} is routing {order}");
                testUntil.Happened();
            }
        }

        private class OrderRouterActor : Router, IRoundRobinOrderRouter
        {
            public OrderRouterActor(int poolSize, TestUntil testUntil)
                : base(
                      new RouterSpecification(
                          poolSize,
                          Definition.Has<OrderRouterWorker>(Definition.Parameters(testUntil)),
                          typeof(IRoundRobinOrderRouter)),
                      new RoundRobinRoutingStrategy())
            {
            }

            public void RouteOrder(RoundRobinOrder order)
            {
                ComputeRouting(order)
                    .RouteesAs<IRoundRobinOrderRouter>()
                    .ToList()
                    .ForEach(orderRoutee => orderRoutee.RouteOrder(order));
            }
        }
    }

    public class RoundRobinOrder
    {
        private readonly int orderId;

        public RoundRobinOrder(int orderId)
        {
            this.orderId = orderId;
        }

        public int OrderId => orderId;

        public override string ToString() => "Order[orderId=" + orderId + "]";
    }

    public interface IRoundRobinOrderRouter
    {
        void RouteOrder(RoundRobinOrder order);
    }
}
