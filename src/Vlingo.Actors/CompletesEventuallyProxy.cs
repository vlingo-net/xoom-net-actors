using System;

namespace Vlingo.Actors
{
    public class CompletesEventuallyProxy : ICompletesEventually
    {
        private const string RepresentationStop1 = "stop()";
        private const string RepresentationWith2 = "with(Object)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public CompletesEventuallyProxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsStopped => actor.IsStopped;

        public void Stop()
        {
            if (!actor.IsStopped)
            {
                Action<IStoppable> consumer = actor => actor.Stop();
                mailbox.Send(new LocalMessage<IStoppable>(actor, consumer, RepresentationStop1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationStop1));
            }
        }

        public void With(object outcome)
        {
            if (!actor.IsStopped)
            {
                Action<ICompletesEventually> consumer = actor => actor.With(outcome);
                mailbox.Send(new LocalMessage<ICompletesEventually>(actor, consumer, RepresentationWith2));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, RepresentationWith2));
            }
        }
    }
}
