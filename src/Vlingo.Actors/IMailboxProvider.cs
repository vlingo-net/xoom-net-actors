namespace Vlingo.Actors
{
    public interface IMailboxProvider
    {
        void Close();
        IMailbox ProvideMailboxFor(int hashCode);
        IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher);
    }
}
