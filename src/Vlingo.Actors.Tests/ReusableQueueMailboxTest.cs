using Xunit;

namespace Vlingo.Actors.Tests
{
    public class ReusableQueueMailboxTest
    {
        private static string QueueMailbox = "queueMailbox";
        private static string ReuseQueueMailbox = "reuseQueueMailbox";
            
        private readonly World _world;
        
        [Fact]
        public void TestThatBothMailboxTypesExist()
        {
            var queueMailboxName = _world.MailboxNameFrom(QueueMailbox);
            Assert.Equal(QueueMailbox, queueMailboxName);
            var queueMailbox = _world.AssignMailbox(queueMailboxName, 1234567);
            Assert.NotNull(queueMailbox);

            var reuseQueueMailboxName = _world.MailboxNameFrom(ReuseQueueMailbox);
            Assert.Equal(ReuseQueueMailbox, reuseQueueMailboxName);
            var reuseQueueMailbox = _world.AssignMailbox(reuseQueueMailboxName, 123456789);
            Assert.NotNull(reuseQueueMailbox);
        }
        
        public ReusableQueueMailboxTest() => _world = World.Start("test-reuse-concurrentqueue");
    }
}