using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    public class Scheduled__Proxy<T> : IScheduled<T>
    {
        private const string IntervalSignalRepresentation1 = "IntervalSignal(Vlingo.Xoom.Common.IScheduled<T>, T)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Scheduled__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public void IntervalSignal(IScheduled<T> scheduled, T data)
        {
            if (!_actor.IsStopped)
            {
                Action<IScheduled<T>> cons1513252312 = __ => __.IntervalSignal(scheduled, data);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1513252312, null, IntervalSignalRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IScheduled<T>>(_actor, cons1513252312,
                        IntervalSignalRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, IntervalSignalRepresentation1));
            }
        }
    }
}