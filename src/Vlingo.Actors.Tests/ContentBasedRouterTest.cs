// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.TestKit;
using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ContentBasedRouterTest : ActorsTest
    {
        [Fact]
        public void TestThatItRoutes()
        {
            var messagesToSend = 63;
            var until = TestUntil.Happenings(messagesToSend);

            var erpsToTest = new[] { ERPSystemCode.Alpha, ERPSystemCode.Beta, ERPSystemCode.Charlie };

            var routerTestActorProtocols = World.ActorFor(
                new[] { typeof(IInvoiceSubmitter), typeof(IInvoiceSubmitterSubscription) },
                typeof(InvoiceSubmissionRouter),
                until);

            var routerProtocols = Protocols.Two<IInvoiceSubmitter, IInvoiceSubmitterSubscription>(routerTestActorProtocols);

            var routerAsInvoiceSubmitter = routerProtocols._1;
            var routerAsInvoiceSubmitterSubscription = routerProtocols._2;

            var alphaSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Alpha, until);
            routerAsInvoiceSubmitterSubscription.Subscribe(alphaSubmitterTestActor.ActorAs<IInvoiceSubmitter>());

            var betaSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Beta, until);
            routerAsInvoiceSubmitterSubscription.Subscribe(betaSubmitterTestActor.ActorAs<IInvoiceSubmitter>());

            var charlieSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Charlie, until);
            routerAsInvoiceSubmitterSubscription.Subscribe(charlieSubmitterTestActor.ActorAs<IInvoiceSubmitter>());

            var random = new Random();
            var countByERP = new int[erpsToTest.Length];
            Array.Fill(countByERP, 0);

            for (var i = 0; i < messagesToSend; ++i)
            {
                var erpIndex = random.Next(3);
                var erp = erpsToTest[erpIndex];
                var invoice = Invoice.With(erp, i, RandomMoney(random));
                routerAsInvoiceSubmitter.SubmitInvoice(invoice);
                countByERP[erpIndex] += 1;
            }

            until.Completes();

            var alphaSubmitter = (ERPSpecificInvoiceSubmitter)alphaSubmitterTestActor.ActorInside;
            Assert.Equal(countByERP[0], alphaSubmitter.submitted.Count);
            foreach (var invoice in alphaSubmitter.submitted)
            {
                Assert.Equal(ERPSystemCode.Alpha, invoice.erp);
            }

            var betaSubmitter = (ERPSpecificInvoiceSubmitter)betaSubmitterTestActor.ActorInside;
            Assert.Equal(countByERP[1], betaSubmitter.submitted.Count);
            foreach (var invoice in betaSubmitter.submitted)
            {
                Assert.Equal(ERPSystemCode.Beta, invoice.erp);
            }

            var charlieSubmitter = (ERPSpecificInvoiceSubmitter)charlieSubmitterTestActor.ActorInside;
            Assert.Equal(countByERP[2], charlieSubmitter.submitted.Count);
            foreach (var invoice in charlieSubmitter.submitted)
            {
                Assert.Equal(ERPSystemCode.Charlie, invoice.erp);
            }
        }

        private decimal RandomMoney(Random random)
        {
            var dollars = random.Next(10_000);
            var cents = random.Next(100);
            return decimal.Parse($"{dollars}.{cents}");
        }

        private class InvoiceSubmissionRouter : ContentBasedRouter<IInvoiceSubmitter>, IInvoiceSubmitter, IInvoiceSubmitterSubscription
        {
            public InvoiceSubmissionRouter(TestUntil testUntil) :
                base(new RouterSpecification(
                    0,
                    Definition.Has<ERPSpecificInvoiceSubmitter>(Definition.Parameters(ERPSystemCode.None, testUntil)),
                    typeof(IInvoiceSubmitter)))
            {
            }

            protected internal override Routing<IInvoiceSubmitter> RoutingFor<T1>(T1 routable1)
                => Routing.With(routees);

            public void Subscribe(IInvoiceSubmitter submitter)
                => Subscribe(Routee<IInvoiceSubmitter>.Of(submitter));

            public void Unsubscribe(IInvoiceSubmitter submitter)
                => Unsubscribe(Routee<IInvoiceSubmitter>.Of(submitter));

            public void SubmitInvoice(Invoice invoice)
                => DispatchCommand(
                    (actor, inv) => actor.SubmitInvoice(inv),
                    invoice);
        }

        private class ERPSpecificInvoiceSubmitter : Actor, IInvoiceSubmitter
        {
            private readonly ERPSystemCode erp;
            private readonly TestUntil testUntil;
            internal readonly List<Invoice> submitted;

            public ERPSpecificInvoiceSubmitter(ERPSystemCode erp, TestUntil testUntil)
            {
                this.erp = erp;
                this.testUntil = testUntil;
                submitted = new List<Invoice>();
            }

            public void SubmitInvoice(Invoice invoice)
            {
                if (erp == invoice.erp)
                {
                    submitted.Add(invoice);
                    testUntil.Happened();
                }
            }
        }
    }

    public interface IInvoiceSubmitter
    {
        void SubmitInvoice(Invoice invoice);
    }

    public interface IInvoiceSubmitterSubscription
    {
        void Subscribe(IInvoiceSubmitter submitter);
        void Unsubscribe(IInvoiceSubmitter submitter);
    }

    public enum ERPSystemCode
    {
        Alpha, Beta, Charlie, None
    }

    public class Invoice
    {
        internal readonly ERPSystemCode erp;
        private readonly int? invoiceId;
        private readonly decimal amount;

        public static Invoice With(ERPSystemCode erp, int? invoiceId, decimal amount)
        {
            return new Invoice(erp, invoiceId, amount);
        }

        internal Invoice(ERPSystemCode erp, int? invoiceId, decimal amount)
        {
            this.erp = erp;
            this.invoiceId = invoiceId;
            this.amount = amount;
        }

        public override int GetHashCode()
        {
            var prime = 31;
            int result = 1;
            result = prime * result + ((invoiceId == null) ? 0 : invoiceId.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;

            Invoice other = (Invoice)obj;
            if (!invoiceId.HasValue)
            {
                if (other.invoiceId.HasValue)
                    return false;
            }
            else if (!invoiceId.Equals(other.invoiceId))
                return false;
            return true;
        }

        public override string ToString()
            => $"Invoice(erp={erp}, invoiceId={invoiceId}, amount={amount})";
    }
}
