using System;

namespace Vlingo.Actors
{
    public class OutcomeAwareProxy<TOutcome, TRef> : IOutcomeAware<TOutcome, TRef>
    {
        private const string FailureOutcomeRepesentation1 = "failureOutcome(Outcome<O>, R)";
        private const string SuccessfulOutcomeRepesentation2 = "successfulOutcome(Outcome<O>, R)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public OutcomeAwareProxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void FailureOutcome(Outcome<TOutcome> outcome, TRef reference)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeAware<TOutcome, TRef>> consumer = actor => actor.FailureOutcome(outcome, reference);
                mailbox.Send(new LocalMessage<IOutcomeAware<TOutcome, TRef>>(actor, consumer, FailureOutcomeRepesentation1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, FailureOutcomeRepesentation1));
            }
        }

        public void SuccessfulOutcome(Outcome<TOutcome> outcome, TRef reference)
        {
            if (!actor.IsStopped)
            {
                Action<IOutcomeAware<TOutcome, TRef>> consumer = actor => actor.SuccessfulOutcome(outcome, reference);
                mailbox.Send(new LocalMessage<IOutcomeAware<TOutcome, TRef>>(actor, consumer, SuccessfulOutcomeRepesentation2));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, SuccessfulOutcomeRepesentation2));
            }
        }
    }
}
