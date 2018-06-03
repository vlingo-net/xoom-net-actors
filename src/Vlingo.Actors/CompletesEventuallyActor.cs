using System;

namespace Vlingo.Actors
{
    public class CompletesEventuallyActor : Actor, ICompletesEventually
    {
        public void With(object outcome)
        {
            try
            {
                var holder = (CompletesHolder)outcome;
                holder.ClientCompletes.With(holder.Outcome);
            }
            catch (Exception ex)
            {
                Logger.Log($"The eventually completed outcome failed in the client because: {ex.Message}", ex);
            }
        }
    }
}
