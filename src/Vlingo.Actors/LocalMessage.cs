using System;

namespace Vlingo.Actors
{
    public class LocalMessage<T> : IMessage
    {
        public LocalMessage(Actor actor, Action<T> consumer, string representation)
        {
            Actor = actor;
            Consumer = consumer;
            Representation = representation;
        }

        public LocalMessage(LocalMessage<T> message)
        {
            Actor = message.Actor;
            Consumer = message.Consumer;
            Representation = message.Representation;
        }

        public Actor Actor { get; }

        public string Representation { get; }

        private Action<T> Consumer { get; }

        public bool IsStowed => false;

        public override string ToString() => $"LocalMessage[{Representation}]";

        public void Deliver()
        {
            if (Actor.LifeCycle.IsResuming)
            {
                if (IsStowed)
                {
                    InternalDeliver(this);
                }
                else
                {
                    InternalDeliver(Actor.LifeCycle.Environment.Suspended.SwapWith(this));
                }
                Actor.LifeCycle.NextResuming();
            }
            else if (Actor.IsDispersing)
            {
                InternalDeliver(Actor.LifeCycle.Environment.Stowage.SwapWith(this));
            }
            else
            {
                InternalDeliver(this);
            }
        }

        private void DeadLetter()
        {
            var deadLetter = new DeadLetter(Actor, Representation);
            var deadLetters = Actor.DeadLetters;
            if(deadLetters != null)
            {
                deadLetters.FailedDelivery(deadLetter);
            }
            else
            {
                Actor.Logger.Log($"vlingo-dotnet/actors: MISSING DEAD LETTERS FOR: {deadLetter}");
            }
        }

        private void InternalDeliver(IMessage message)
        {
            if (Actor.IsStopped)
            {
                DeadLetter();
            }
            else if (Actor.LifeCycle.IsSuspended)
            {
                Actor.LifeCycle.Environment.Suspended.Stow(message);
            }
            else if (Actor.IsStowing)
            {
                Actor.LifeCycle.Environment.Stowage.Stow(message);
            }
            else
            {
                try
                {
                    Consumer.Invoke((T)(object)Actor);
                }
                catch(Exception ex)
                {
                    Actor.Logger.Log($"Message#Deliver(): Exception: {ex.Message} for Actor: {Actor} sending: {Representation}", ex);
                    Actor.Stage.HandleFailureOf<T>(new StageSupervisedActor<T>(Actor, ex));
                }
            }
        }
    }
}
