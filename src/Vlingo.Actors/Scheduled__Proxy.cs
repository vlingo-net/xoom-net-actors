using System;

namespace Vlingo.Actors
{
    public class Scheduled__Proxy : IScheduled
    {
        private const string RepresentationIntervalSignal1 = "IntervalSignal(IScheduled, object)";
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Scheduled__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void IntervalSignal(IScheduled scheduled, object data)
        {
            if (!actor.IsStopped)
            {
                Action<IScheduled> consumer = actor => actor.IntervalSignal(scheduled, data);
                mailbox.Send(new LocalMessage<IScheduled>(actor, consumer, RepresentationIntervalSignal1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationIntervalSignal1));
            }
        }
    }
}
