namespace Vlingo.Actors
{
    public interface IRegistrar
    {
        void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider);
        void Register(string name, bool isDefault, ILoggerProvider loggerProvider);
        void Register(string name, bool isDefault, IMailboxProvider mailboxProvider);
        void RegisterCommonSupervisor(string stageName, string name, string fullyQualifiedProtocol, string fullyQualifiedSupervisor);
        void RegisterDefaultSupervisor(string stageName, string name, string fullyQualifiedSupervisor);
        World World { get; }
    }
}
