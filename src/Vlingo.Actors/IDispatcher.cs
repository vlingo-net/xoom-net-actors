namespace Vlingo.Actors
{
    public interface IDispatcher
    {
        bool IsClosed { get; }
        void Close();
        bool RequiresExecutionNotification { get; }
        void Execute(IMailbox mailbox);
    }
}
