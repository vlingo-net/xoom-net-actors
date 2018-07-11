namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    class ConcurrentQueueMailboxSettings
    {
        internal readonly int throttlingCount;

        internal static ConcurrentQueueMailboxSettings Instance { get; private set; }

        internal static void With(int throttlingCount)
        {
            Instance = new ConcurrentQueueMailboxSettings(throttlingCount);
        }

        private ConcurrentQueueMailboxSettings(int throttlingCount)
        {
            this.throttlingCount = throttlingCount <= 0 ? 1 : throttlingCount;
        }
    }
}
