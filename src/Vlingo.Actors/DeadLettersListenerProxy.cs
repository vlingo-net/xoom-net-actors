using System;

namespace Vlingo.Actors
{
    public class DeadLettersListenerProxy : IDeadLettersListener
    {
        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public DeadLettersListenerProxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void Handle(DeadLetter deadLetter)
        {
            if (!actor.IsStopped)
            {
                Action<IDeadLettersListener> consumer = actor => actor.Handle(deadLetter);
                mailbox.Send(new LocalMessage<IDeadLettersListener>(actor, consumer, "Handle(DeadLetter)"));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, "Handle(DeadLetter)"));
            }
        }
    }
}
