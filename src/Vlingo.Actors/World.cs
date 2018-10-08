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

        private CompletesEventuallyProviderKeeper completesProviderKeeper;
        private LoggerProviderKeeper loggerProviderKeeper;
        private MailboxProviderKeeper mailboxProviderKeeper;
        private IDictionary<string, Stage> stages;
        private ILogger defaultLogger;
        private ISupervisor defaultSupervisor;
        private DynaClassLoader classLoader;

        private World(string name, bool forceDefaultConfiguration)
        {
            Name = name;
            classLoader = new DynaClassLoader(GetType().GetAssemblyLoadContext());
            completesProviderKeeper = new CompletesEventuallyProviderKeeper();
            loggerProviderKeeper = new LoggerProviderKeeper();
            mailboxProviderKeeper = new MailboxProviderKeeper();
            stages = new Dictionary<string, Stage>();

            AddressFactory = new AddressFactory();

            var defaultStage = StageNamed(DefaultStage);

            var pluginLoader = new PluginLoader();

            pluginLoader.LoadEnabledPlugins(this, 1, forceDefaultConfiguration);

            StartRootFor(defaultStage, DefaultLogger);

            pluginLoader.LoadEnabledPlugins(this, 2, forceDefaultConfiguration);
        }

        public AddressFactory AddressFactory { get; }

        public static World Start(string name)
        {
            return Start(name, false);
        }

        private static readonly object startMutex = new object();
        public static World Start(string name, bool forceDefaultConfiguration = false)
        {
            lock (startMutex)
            {
                if (name == null)
                {
                    throw new ArgumentException("The world name must not be null.");
                }

                return new World(name, forceDefaultConfiguration);
            }
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

        public ICompletesEventually CompletesFor<T>(ICompletes<T> clientCompletes)
            => completesProviderKeeper.FindDefault().ProvideCompletesFor<T>(clientCompletes);

        public ILogger DefaultLogger
        {
            get
            {
                if (defaultLogger != null)
                {
                    return defaultLogger;
                }
                defaultLogger = loggerProviderKeeper.FindDefault().Logger;

                if(defaultLogger == null)
                {
                    defaultLogger = LoggerProvider.StandardLoggerProvider(this, "vlingo").Logger;
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
            loggerProviderKeeper.Keep(name, isDefault, loggerProvider);
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
                var supervisorClass = classLoader.LoadClass(fullyQualifiedSupervisor);
                var common = stage.ActorFor<ISupervisor>(Definition.Has(supervisorClass, Definition.NoParameters, name));
                stage.RegisterCommonSupervisor(fullyQualifiedProtocol, common);
            }
            catch (Exception e)
            {
                DefaultLogger.Log($"vlingo-net/actors: World cannot register common supervisor: {fullyQualifiedSupervisor}", e);
            }
        }

        public virtual void RegisterDefaultSupervisor(string stageName, string name, Type supervisorClass)
        {
            try
            {
                var actualStageName = stageName.Equals("default") ? DefaultStage : stageName;
                var stage = StageNamed(actualStageName);
                var supervisorClass = classLoader.LoadClass(fullyQualifiedSupervisor);
                defaultSupervisor = stage.ActorFor<ISupervisor>(Definition.Has(supervisorClass, Definition.NoParameters, name));
            }
            catch (Exception e)
            {
                DefaultLogger.Log("vlingo-net/actors: World cannot register default supervisor override: {fullyQualifiedSupervisor}", e);
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
            => stage.ActorFor<IStoppable>(
                Definition.Has<PrivateRootActor>(Definition.NoParameters, PrivateRootName),
                null,
                AddressFactory.AddressFrom(PrivateRootId, PrivateRootName),
                null,
                null,
                logger);

    }
}