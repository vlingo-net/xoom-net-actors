using System.Collections.Concurrent;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailbox : IMailbox, IRunnable
    {
        private readonly IDispatcher dispatcher;
        private readonly AtomicBoolean delivering;
        private readonly ConcurrentQueue<IMessage> queue;

        internal ConcurrentQueueMailbox(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            delivering = new AtomicBoolean(false);
            queue = new ConcurrentQueue<IMessage>();
        }

        public void Close()
        {
            queue.Clear();
            dispatcher.Close();
        }

        public bool IsClosed => dispatcher.IsClosed;

        public void Send(IMessage message)
        {
            queue.Enqueue(message);
            if (!IsDelivering)
            {
                dispatcher.Execute(this);
            }
        }

        public IMessage Receive()
        {
            if(queue.TryDequeue(out IMessage result))
            {
                return result;
            }

            return null;
        }

        public bool IsDelivering => delivering.Get();

        public bool Delivering(bool flag) => delivering.CompareAndSet(!flag, flag);

        public void Run()
        {
            var total = ConcurrentQueueMailboxSettings.Instance.throttlingCount;
            for(var count = 0; count < total; ++count)
            {
                var message = Receive();
                if(message != null)
                {
                    message.Deliver();
                }
                else
                {
                    break;
                }
            }
            Delivering(false);

            if (!queue.IsEmpty)
            {
                dispatcher.Execute(this);
            }
        }
    }
}
