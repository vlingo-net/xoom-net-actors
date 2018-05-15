using System;

namespace Vlingo.Actors
{
    public class World
    {
        public static int PrivateRootId = int.MaxValue;
        public static int PublicRootId = PrivateRootId - 1;
        public static int DeadlettersId = PublicRootId - 1;
        private const string DEFAULT_STAGE = "__defaultStage";

        private ISupervisor defaultSupervisor;

        public DeadLetters DeadLetters { get; set; }

        public Stage Stage => StageNamed(DEFAULT_STAGE);

        internal Actor DefaultParent { get; }

        internal ISupervisor DefaultSupervisor => defaultSupervisor ?? DefaultParent.SelfAs<ISupervisor>();

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
    }
}