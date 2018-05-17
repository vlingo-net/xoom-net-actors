using System;

namespace Vlingo.Actors
{
    public class DeadLettersProxy : IDeadLetters
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public DeadLettersProxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsStopped => actor.IsStopped;

        public void Stop()
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.Stop();
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "Stop()"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Stop()"));
            }
        }

        public void FailedDelivery(DeadLetter deadLetter)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.FailedDelivery(deadLetter);
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "FailedDelivery(DeadLetter)"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "FailedDelivery(DeadLetter)"));
            }
        }

        public void RegisterListener(IDeadLettersListener listener)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLetters> consumer = actor => actor.RegisterListener(listener);
                mailbox.Send(new LocalMessage<IDeadLetters>(actor, consumer, "RegisterListener(DeadLettersListener)"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "RegisterListener(DeadLettersListener)"));
            }
        }
    }
}
