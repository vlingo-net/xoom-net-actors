// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Actors.Plugin;
using Vlingo.Common.Compiler;

namespace Vlingo.Actors
{
    public class World : IRegistrar
    {
        internal const int PrivateRootId = int.MaxValue;
        internal const string PrivateRootName = "#private";
        internal const int PublicRootId = PrivateRootId - 1;
        internal const string PublicRootName = "#public";
        internal const int DeadLettersId = PublicRootId - 1;
        internal const string DeadLettersName = "#deadLetters";
        internal const int HighRootId = DeadLettersId - 1;
        internal const string DefaultStage = "__defaultStage";

        private readonly CompletesEventuallyProviderKeeper completesProviderKeeper;
        private readonly LoggerProviderKeeper loggerProviderKeeper;
        private readonly MailboxProviderKeeper mailboxProviderKeeper;

        private IDictionary<string, Stage> stages;
        private ILogger defaultLogger;
        private ISupervisor defaultSupervisor;
        private readonly DynaClassLoader classLoader;

        private World(string name, Configuration configuration)
        {
            Name = name;
            Configuration = configuration;
            classLoader = new DynaClassLoader(GetType().GetAssemblyLoadContext());
            completesProviderKeeper = new CompletesEventuallyProviderKeeper();
            loggerProviderKeeper = new LoggerProviderKeeper();
            mailboxProviderKeeper = new MailboxProviderKeeper();
            stages = new Dictionary<string, Stage>();

            AddressFactory = new AddressFactory();

            var defaultStage = StageNamed(DefaultStage);

            configuration.StartPlugins(this, 1);

            StartRootFor(defaultStage, DefaultLogger);

            configuration.StartPlugins(this, 2);
            defaultStage.StartDirectoryScanner();
        }

        public AddressFactory AddressFactory { get; }

        public Configuration Configuration { get; }

        public static World Start(string name)
        {
            return Start(name, Properties.Instance);
        }

        public static World Start(string name, Properties properties)
        {
            return Start(name, Configuration.DefineWith(properties));
        }

        private static readonly object startMutex = new object();
        public static World Start(string name, Configuration configuration)
        {
            lock (startMutex)
            {
                if (name == null)
                {
                    throw new ArgumentException("The world name must not be null.");
                }

                return new World(name, configuration);
            }
        }

        public static World StartWithDefault(string name)
        {
            return Start(name, Configuration.Define());
        }

        public T ActorFor<T>(Definition definition)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo/actors: Stopped.");
            }

            return Stage.ActorFor<T>(definition);
        }

        public Protocols ActorFor(Definition definition, Type[] protocols)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo/actors: Stopped.");
            }

            return Stage.ActorFor(definition, protocols);
        }

        public IDeadLetters DeadLetters { get; internal set; }

        public ICompletesEventually CompletesFor(ICompletes clientCompletes)
            => completesProviderKeeper.FindDefault().ProvideCompletesFor(clientCompletes);

        public ILogger DefaultLogger
        {
            get
            {
                if (defaultLogger != null)
                {
                    return defaultLogger;
                }

                if (loggerProviderKeeper != null)
                {
                    var maybeLoggerProvider = loggerProviderKeeper.FindDefault();
                    defaultLogger = maybeLoggerProvider != null ?
                        maybeLoggerProvider.Logger :
                        LoggerProvider.NoOpLoggerProvider().Logger;
                }

                if (defaultLogger == null)
                {
                    defaultLogger = LoggerProvider.StandardLoggerProvider(this, "vlingo-net").Logger;
                }

                return defaultLogger;
            }
        }

        public Actor DefaultParent { get; private set; }

        public ISupervisor DefaultSupervisor
        {
            get
            {
                if(defaultSupervisor == null)
                {
                    defaultSupervisor = DefaultParent.SelfAs<ISupervisor>();
                }

                return defaultSupervisor;
            }
        }

        public ILogger Logger(string name) => loggerProviderKeeper.FindNamed(name).Logger;

        public string Name { get; }

        public virtual void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            completesEventuallyProvider.InitializeUsing(Stage);
            completesProviderKeeper.Keep(name, completesEventuallyProvider);
        }

        public virtual void Register(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            var actualDefault = loggerProviderKeeper.FindDefault() == null ? true : isDefault;
            loggerProviderKeeper.Keep(name, actualDefault, loggerProvider);
            defaultLogger = loggerProviderKeeper.FindDefault().Logger;
        }

        public virtual void Register(string name, bool isDefault, IMailboxProvider mailboxProvider)
            => mailboxProviderKeeper.Keep(name, isDefault, mailboxProvider);

        public virtual void RegisterCommonSupervisor(string stageName, string name, Type supervisedProtocol, Type supervisorClass)
        {
            try
            {
                var actualStageName = stageName.Equals("default") ? DefaultStage : stageName;
                var stage = StageNamed(actualStageName);
                var common = stage.ActorFor<ISupervisor>(Definition.Has(supervisorClass, Definition.NoParameters, name));
                stage.RegisterCommonSupervisor(supervisedProtocol, common);
            }
            catch (Exception e)
            {
                DefaultLogger.Log($"vlingo-net/actors: World cannot register common supervisor: {supervisedProtocol.Name}", e);
            }
        }

        public virtual void RegisterDefaultSupervisor(string stageName, string name, Type supervisorClass)
        {
            try
            {
                var actualStageName = stageName.Equals("default") ? DefaultStage : stageName;
                var stage = StageNamed(actualStageName);
                defaultSupervisor = stage.ActorFor<ISupervisor>(Definition.Has(supervisorClass, Definition.NoParameters, name));
            }
            catch (Exception e)
            {
                DefaultLogger.Log($"vlingo-net/actors: World cannot register default supervisor override: {supervisorClass.Name}", e);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public Stage Stage => StageNamed(DefaultStage);

        private readonly object stageNamedMutex = new object();
        public Stage StageNamed(string name)
        {
            lock (stageNamedMutex)
            {
                if(!stages.TryGetValue(name, out Stage stage))
                {
                    stage = new Stage(this, name);
                    if(!string.Equals(name, DefaultStage))
                    {
                        stage.StartDirectoryScanner();
                    }
                    stages[name] = stage;
                }

                return stage;
            }
        }

        public virtual bool IsTerminated => Stage.IsStopped;

        public virtual void Terminate()
        {
            if (!IsTerminated)
            {
                foreach (var stage in stages.Values)
                {
                    stage.Stop();
                }

                loggerProviderKeeper.Close();
                mailboxProviderKeeper.Close();
                completesProviderKeeper.Close();
            }
        }

        World IRegistrar.World => this;

        internal IMailbox AssignMailbox(string mailboxName, int hashCode)
            => mailboxProviderKeeper.AssignMailbox(mailboxName, hashCode);

        internal string MailboxNameFrom(string candidateMailboxName)
        {
            if (candidateMailboxName == null)
            {
                return FindDefaultMailboxName();
            }
            else if (mailboxProviderKeeper.IsValidMailboxName(candidateMailboxName))
            {
                return candidateMailboxName;
            }
            else
            {
                return FindDefaultMailboxName();
            }
        }

        internal String FindDefaultMailboxName()
        {
            return mailboxProviderKeeper.FindDefault();
        }

        private readonly object defaultParentMutex = new object();
        internal void SetDefaultParent(Actor defaultParent)
        {
            lock (defaultParentMutex)
            {
                if(defaultParent != null && DefaultParent != null)
                {
                    throw new InvalidOperationException("Default parent already exists.");
                }

                DefaultParent = defaultParent;
            }
        }

        private readonly object deadLettersMutex = new object();
        internal void SetDeadLetters(IDeadLetters deadLetters)
        {
            lock (deadLettersMutex)
            {
                if(deadLetters != null && DeadLetters != null)
                {
                    deadLetters.Stop();
                    throw new InvalidOperationException("Dead letters already exists.");
                }

                DeadLetters = deadLetters;
            }
        }

        internal IStoppable PrivateRoot { get; private set; }

        private readonly object privateRootMutex = new object();
        internal void SetPrivateRoot(IStoppable privateRoot)
        {
            lock (privateRootMutex)
            {
                if (privateRoot != null && PrivateRoot != null)
                {
                    privateRoot.Stop();
                    throw new InvalidOperationException("Private root already exists.");
                }

                PrivateRoot = privateRoot; 
            }
        }

        internal IStoppable PublicRoot { get; private set; }

        public readonly object publicRootMutex = new object();
        internal void SetPublicRoot(IStoppable publicRoot)
        {
            lock (publicRootMutex)
            {
                if (publicRoot != null && PublicRoot != null)
                {
                    throw new InvalidOperationException("The public root already exists.");
                }

                PublicRoot = publicRoot;
            }
        }

        private void StartRootFor(Stage stage, ILogger logger)
            => stage.ActorProtocolFor<IStoppable>(
                Definition.Has<PrivateRootActor>(Definition.NoParameters, PrivateRootName),
                null,
                AddressFactory.From(PrivateRootId, PrivateRootName),
                null,
                null,
                logger);
    }
}