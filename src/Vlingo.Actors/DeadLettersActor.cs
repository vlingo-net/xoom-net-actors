using System;
using System.Collections.Generic;

namespace Vlingo.Actors
{
    public class DeadLettersActor : Actor, IDeadLetters
    {
        private readonly IList<IDeadLettersListener> listeners;

        public DeadLettersActor()
        {
            listeners = new List<IDeadLettersListener>();
            Stage.World.DeadLetters = SelfAs<IDeadLetters>();
        }

        public void FailedDelivery(DeadLetter deadLetter)
        {
            Logger.Log($"vlingo-dotnet/actors: {deadLetter}");

            foreach (var listener in listeners)
            {
                try
                {
                    listener.Handle(deadLetter);
                }
                catch (Exception ex)
                {
                    // ignore, but log
                    Logger.Log($"DeadLetters listener failed to handle: {deadLetter}", ex);
                }
            }
        }

        public void RegisterListener(IDeadLettersListener listener)
        {
            listeners.Add(listener);
        }

        internal override void AfterStop()
        {
            Stage.World.DeadLetters = null;
            base.AfterStop();
        }
    }
}