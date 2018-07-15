// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;

namespace Vlingo.Actors
{
    public class World : IRegistrar
    {
        public static int PrivateRootId = int.MaxValue;
        public static int PublicRootId = PrivateRootId - 1;
        public static int DeadlettersId = PublicRootId - 1;
        private const string DEFAULT_STAGE = "__defaultStage";

        public static World Start(string name)
        {
            return Start(name, false);
        }

        public static World Start(string name, bool forceDefaultConfiguration)
        {
            if (name == null)
            {
                throw new ArgumentException("The world name must not be null.");
            }

            return new World(name, forceDefaultConfiguration);
        }

        internal void SetPrivateRoot(IStoppable privateRoot)
        {
            throw new NotImplementedException();
        }

        private ISupervisor defaultSupervisor;

        public IDeadLetters DeadLetters { get; set; }

        public CompletesEventually CompletesFor<T>(ICompletes<T> clientCompletes)
        {
            throw new NotImplementedException();
        }

        public Stage Stage => StageNamed(DEFAULT_STAGE);

        internal Actor DefaultParent { get; }

        internal ISupervisor DefaultSupervisor => defaultSupervisor ?? DefaultParent.SelfAs<ISupervisor>();

        World IRegistrar.World => throw new NotImplementedException();

        public string Name { get; internal set; }
        public bool IsTerminated { get; }
        public ILogger DefaultLogger { get; internal set; }

        public const string PUBLIC_ROOT_NAME = "#public";
        public const int PUBLIC_ROOT_ID = PRIVATE_ROOT_ID - 1;
        public const int PRIVATE_ROOT_ID = int.MaxValue;

        public World(string name, bool forceDefaultConfiguration)
        {
        }

        public Stage StageNamed(string name)
        {
            throw new System.NotImplementedException();
        }

        internal IMailbox MailboxNameFrom(string mailboxName)
        {
            throw new NotImplementedException();
        }

        internal ILogger Logger(string name)
        {
            throw new NotImplementedException();
        }

        internal IMailbox AssignMailbox(object mailboxName, int v)
        {
            throw new NotImplementedException();
        }

        internal void Terminate()
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