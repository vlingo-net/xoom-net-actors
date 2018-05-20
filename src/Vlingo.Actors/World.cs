using System;

namespace Vlingo.Actors
{
    public class World : IRegistrar
    {
        public static int PrivateRootId = int.MaxValue;
        public static int PublicRootId = PrivateRootId - 1;
        public static int DeadlettersId = PublicRootId - 1;
        private const string DEFAULT_STAGE = "__defaultStage";

        private ISupervisor defaultSupervisor;

        public IDeadLetters DeadLetters { get; set; }

        public Stage Stage => StageNamed(DEFAULT_STAGE);

        internal Actor DefaultParent { get; }

        internal ISupervisor DefaultSupervisor => defaultSupervisor ?? DefaultParent.SelfAs<ISupervisor>();

        World IRegistrar.World => throw new NotImplementedException();

        public Stage StageNamed(string name)
        {
            throw new System.NotImplementedException();
        }

        internal IMailbox MailboxNameFrom(string mailboxName)
        {
            throw new NotImplementedException();
        }

        internal IMailbox AssignMailbox(object mailboxName, int v)
        {
            throw new NotImplementedException();
        }

        public void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            throw new NotImplementedException();
        }

        public void Register(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            throw new NotImplementedException();
        }

        public void Register(string name, bool isDefault, IMailboxProvider mailboxProvider)
        {
            throw new NotImplementedException();
        }

        public void RegisterCommonSupervisor(string stageName, string name, string fullyQualifiedProtocol, string fullyQualifiedSupervisor)
        {
            throw new NotImplementedException();
        }

        public void RegisterDefaultSupervisor(string stageName, string name, string fullyQualifiedSupervisor)
        {
            throw new NotImplementedException();
        }
    }
}