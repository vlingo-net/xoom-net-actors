using System;

namespace Vlingo.Actors
{
    public class CompletesEventuallyActor : Actor, ICompletesEventually
    {
        public void With(object outcome)
        {
            try
            {
                var pooled = (PooledCompletes)outcome;
                pooled.ClientCompletes.With(pooled.Outcome);
            }
            catch (Exception ex)
            {
                Logger.Log($"The eventually completed outcome failed in the client because: {ex.Message}", ex);
            }
        }

        public override bool IsStopped => false;

        public override void Stop()
        {
        }
    }
}
