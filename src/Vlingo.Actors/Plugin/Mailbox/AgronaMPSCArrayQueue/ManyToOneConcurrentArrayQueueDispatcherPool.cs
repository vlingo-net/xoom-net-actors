using System;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    internal class ManyToOneConcurrentArrayQueueDispatcherPool
    {
        private readonly ManyToOneConcurrentArrayQueueDispatcher[] dispatchers;

        internal ManyToOneConcurrentArrayQueueDispatcherPool(
            int availableThreads,
            float numberOfDispatchersFactor,
            int ringBufferSize,
            long fixedBackoff,
            int throttlingCount,
            int totalSendRetries)
        {
            var numberOfDispatchers = (int)(availableThreads * numberOfDispatchersFactor);
            dispatchers = new ManyToOneConcurrentArrayQueueDispatcher[numberOfDispatchers];

            for (int idx = 0; idx < dispatchers.Length; ++idx)
            {
                var dispatcher =
                        new ManyToOneConcurrentArrayQueueDispatcher(
                                ringBufferSize,
                                fixedBackoff,
                                throttlingCount,
                                totalSendRetries);

                dispatcher.Start();
                dispatchers[idx] = dispatcher;
            }
        }

        internal ManyToOneConcurrentArrayQueueDispatcher AssignFor(int hashCode)
        {
            var index = Math.Abs(hashCode) % dispatchers.Length;
            return dispatchers[index];
        }

        internal void Close()
        {
            for (int idx = 0; idx < dispatchers.Length; ++idx)
            {
                dispatchers[idx].Close();
            }
        }
    }
}
