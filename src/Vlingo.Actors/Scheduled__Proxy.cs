using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Common
{
    public class Scheduled__Proxy<T> : Vlingo.Common.IScheduled<T>
    {
        private const string IntervalSignalRepresentation1 = "IntervalSignal(Vlingo.Common.IScheduled<T>, T)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Scheduled__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void IntervalSignal(Vlingo.Common.IScheduled<T> scheduled, T data)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Common.IScheduled<T>> cons1513252312 = __ => __.IntervalSignal(scheduled, data);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1513252312, null, IntervalSignalRepresentation1);
                }
                else
                {
                    this.mailbox.Send(new LocalMessage<Vlingo.Common.IScheduled<T>>(this.actor, cons1513252312,
                        IntervalSignalRepresentation1));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, IntervalSignalRepresentation1));
            }
        }
    }
}