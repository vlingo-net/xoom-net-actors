using System;

namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailbox : IMailbox
    {
        public bool IsClosed => throw new NotImplementedException();

        public bool IsDelivering => throw new NotImplementedException();

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Delivering(bool flag)
        {
            throw new NotImplementedException();
        }

        public IMessage Receive()
        {
            throw new NotImplementedException();
        }

        public void Send(IMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
