// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ContentBasedRoutingStrategyTest
    {
        [Fact]
        public void TestThatItRoutes()
        {
            var world = World.StartWithDefault("ContentBasedRouterTest");
            const int poolSize = 4, messagesToSend = 40;
            var until = TestUntil.Happenings(messagesToSend);

            var orderRouter = world.ActorFor<IOrderContentRouter>(Definition.Has<OrderRouterActor>(Definition.Parameters(poolSize, until)));
            var customerIds = new[] { "Customer1", "Customer2", "Customer3", "Customer4" };
            var random = new Random();

            for (int i = 0; i < messagesToSend; i++)
            {
                var customerId = customerIds[random.Next(customerIds.Length)];
                orderRouter.RouteOrder(new Order(customerId));
            }

            until.Completes();
        }

        private class ContentBasedRoutingStrategy : RoutingStrategyAdapter
        {
            public override Routing ChooseRouteFor<T1>(T1 routable1, IList<Routee> routees)
            {
                var order = routable1 as Order;
                var customerId = order.CustomerId;

                return customerId == "Customer1" ?
                    Routing.With(routees[0]) :
                    Routing.With(routees[1]);
            }
        }

        private class OrderRouterWorker : Actor, IOrderContentRouter
        {
            private readonly TestUntil testUntil;

            public OrderRouterWorker(TestUntil testUntil)
            {
                this.testUntil = testUntil;
            }

            public void RouteOrder(Order order)
            {
                Logger.Log($"{ToString()} is routing {order}");
                testUntil.Happened();
            }
        }

        private class OrderRouterActor : Router, IOrderContentRouter
        {
            public OrderRouterActor(int poolSize, TestUntil testUntil)
                : base(
                      new RouterSpecification(poolSize, Definition.Has<OrderRouterWorker>(Definition.Parameters(testUntil)), typeof(IOrderContentRouter)),
                      new ContentBasedRoutingStrategy())
            {
            }

            public void RouteOrder(Order order)
            {
                ComputeRouting(order)
                    .RouteesAs<IOrderContentRouter>()
                    .ToList()
                    .ForEach(x => x.RouteOrder(order));
            }
        }
    }

    public interface IOrderContentRouter
    {
        void RouteOrder(Order order);
    }

    public class Order
    {
        public Order(string customerId)
        {
            OrderId = Guid.NewGuid().ToString();
            CustomerId = customerId;
        }

        public string OrderId { get; }

        public string CustomerId { get; }

        public override string ToString()
        {
            return $"Order[orderId={OrderId}, customerId={CustomerId}]";
        }
    }
}
