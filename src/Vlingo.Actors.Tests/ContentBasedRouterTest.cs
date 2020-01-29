// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
            var testResults = new TestResults(messagesToSend);

            var erpsToTest = new[] { ERPSystemCode.Alpha, ERPSystemCode.Beta, ERPSystemCode.Charlie };

            var routerTestActorProtocols = World.ActorFor(
                new[] { typeof(IInvoiceSubmitter), typeof(IInvoiceSubmitterSubscription) },
                typeof(InvoiceSubmissionRouter),
                testResults);

            var routerProtocols = Protocols.Two<IInvoiceSubmitter, IInvoiceSubmitterSubscription>(routerTestActorProtocols);

            var routerAsInvoiceSubmitter = routerProtocols._1;
            var routerAsInvoiceSubmitterSubscription = routerProtocols._2;

            var alphaSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Alpha, testResults);
            routerAsInvoiceSubmitterSubscription.Subscribe(alphaSubmitterTestActor.ActorAs<IInvoiceSubmitter>());

            var betaSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Beta, testResults);
            routerAsInvoiceSubmitterSubscription.Subscribe(betaSubmitterTestActor.ActorAs<IInvoiceSubmitter>());

            var charlieSubmitterTestActor = TestWorld.ActorFor<IInvoiceSubmitter>(typeof(ERPSpecificInvoiceSubmitter), ERPSystemCode.Charlie, testResults);
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

            AssertSubmittedInvoices(testResults, erpsToTest, countByERP, ERPSystemCode.Alpha);
            AssertSubmittedInvoices(testResults, erpsToTest, countByERP, ERPSystemCode.Beta);
            AssertSubmittedInvoices(testResults, erpsToTest, countByERP, ERPSystemCode.Charlie);
        }

        private void AssertSubmittedInvoices(TestResults testResults, ERPSystemCode[] erpsToTest, int[] countByERP, ERPSystemCode systemCode)
        {
            var submittedInvoices = testResults.GetSubmittedInvoices(systemCode);
            Assert.Equal(countByERP[Array.BinarySearch(erpsToTest, systemCode)], submittedInvoices.Count);

            foreach(var invoice in submittedInvoices)
            {
                Assert.Equal(systemCode, invoice.erp);
            }
        }

        private decimal RandomMoney(Random random)
        {
            var dollars = random.Next(10_000);
            var cents = random.Next(100);
            return decimal.Parse($"{dollars}.{cents}", CultureInfo.InvariantCulture);
        }

        private class InvoiceSubmissionRouter : ContentBasedRouter<IInvoiceSubmitter>, IInvoiceSubmitter, IInvoiceSubmitterSubscription
        {
            public InvoiceSubmissionRouter(TestResults testResults) :
                base(new RouterSpecification<IInvoiceSubmitter>(
                    0,
                    Definition.Has<ERPSpecificInvoiceSubmitter>(Definition.Parameters(ERPSystemCode.None, testResults))))
            {
            }

            protected internal override Routing<IInvoiceSubmitter> RoutingFor<T1>(T1 routable1)
            {
                if (routable1 is Invoice)
                {
                    return Routing.With(routees);
                }
                else
                {
                    return Routing.With(new List<Routee<IInvoiceSubmitter>>(0));
                }
            }

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
            private readonly TestResults testResults;

            public ERPSpecificInvoiceSubmitter(ERPSystemCode erp, TestResults testResults)
            {
                this.erp = erp;
                this.testResults = testResults;
            }

            public void SubmitInvoice(Invoice invoice)
            {
                if (erp == invoice.erp)
                {
                    testResults.InvoiceSubmitted(invoice);
                }
            }
        }

        private class TestResults
        {
            internal readonly AccessSafely submittedInvoices;

            public TestResults(int times)
            {
                var invoices = new ConcurrentDictionary<ERPSystemCode, IList<Invoice>>();
                submittedInvoices = AccessSafely.AfterCompleting(times);
                submittedInvoices.WritingWith<ERPSystemCode, Invoice>(
                    "submittedInvoices",
                    (key, value) => invoices.GetOrAdd(key, new List<Invoice>()).Add(value));
                submittedInvoices.ReadingWith<ERPSystemCode, IList<Invoice>>("submittedInvoices", key => invoices.GetValueOrDefault(key));
            }

            public void InvoiceSubmitted(Invoice invoice) 
                => submittedInvoices.WriteUsing("submittedInvoices", invoice.erp, invoice);

            public IList<Invoice> GetSubmittedInvoices(ERPSystemCode code) 
                => submittedInvoices.ReadFrom<ERPSystemCode, IList<Invoice>>("submittedInvoices", code);
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
