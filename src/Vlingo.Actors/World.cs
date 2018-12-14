// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vlingo.Actors.Plugin.Completes;
using Vlingo.Actors.Plugin.Logging;
using Vlingo.Actors.Plugin.Mailbox;
using Vlingo.Common;
using Vlingo.Common.Compiler;

namespace Vlingo.Actors
{
    /// <summary>
    /// The <c>World</c> of the actor runtime through which all Stage and Actor instances are created and run.
    /// All plugins and all default facilities are registered through the <c>World</c>.
    /// </summary>
    public sealed class World : IRegistrar
    {
        internal const long PrivateRootId = long.MaxValue;
        internal const string PrivateRootName = "#private";
        internal const long PublicRootId = PrivateRootId - 1;
        internal const string PublicRootName = "#public";
        internal const long DeadLettersId = PublicRootId - 1;
        internal const string DeadLettersName = "#deadLetters";
        public const long HighRootId = DeadLettersId - 1;
        internal const string DefaultStage = "__defaultStage";

        private readonly IDictionary<string, object> dynamicDependencies;
        private readonly IDictionary<string, Stage> stages;

        private ICompletesEventuallyProviderKeeper completesProviderKeeper;
        private ILoggerProviderKeeper loggerProviderKeeper;
        private IMailboxProviderKeeper mailboxProviderKeeper;

        private ILogger defaultLogger;
        private ISupervisor defaultSupervisor;

        /// <summary>
        /// Initializes the new <c>World</c> instance with the given name and configuration.
        /// </summary>
        /// <param name="name">The <c>string</c> name to assign to this <c>World</c>.</param>
        /// <param name="configuration">the <c>Configuration</c> to use to initialize various <c>World</c> facilities.</param>
        private World(string name, Configuration configuration)
        {
            Name = name;
            Configuration = configuration;
            AddressFactory = new BasicAddressFactory();
            completesProviderKeeper = new DefaultCompletesEventuallyProviderKeeper();
            loggerProviderKeeper = new DefaultLoggerProviderKeeper();
            mailboxProviderKeeper = new DefaultMailboxProviderKeeper();
            stages = new ConcurrentDictionary<string, Stage>();
            dynamicDependencies = new ConcurrentDictionary<string, object>();

            var defaultStage = StageNamed(DefaultStage);

            configuration.StartPlugins(this, 0);
            configuration.StartPlugins(this, 1);

            StartRootFor(defaultStage, DefaultLogger);

            configuration.StartPlugins(this, 2);
            defaultStage.StartDirectoryScanner();
        }

        /// <summary>
        /// Gets the Address Factory for this <c>World</c>.
        /// </summary>
        public IAddressFactory AddressFactory { get; }

        /// <summary>
        /// Gets the <c>Configuration</c> of this <c>World</c>
        /// </summary>
        public Configuration Configuration { get; }

        /// <summary>
        /// Answers a new <c>World</c> with the given <paramref name="name"/> and that is configured with
        /// the contents of the <c>vlingo-actors.properties</c> file.
        /// </summary>
        /// <param name="name">the <c>string</c> name to assign to the new <c>World</c> instance.</param>
        /// <returns>A <c>World</c> instance.</returns>
        public static World Start(string name)
        {
            return Start(name, Properties.Instance);
        }

        /// <summary>
        /// Answers a new <c>World</c> with the given <paramref name="name"/> and that is configured with
        /// the contents of the <paramref name="properties"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> name to assign to the new <c>World</c> instance.</param>
        /// <param name="properties">The <see cref="Properties"/> used for configuration.</param>
        /// <returns>A <c>World</c> instance.</returns>
        public static World Start(string name, Properties properties)
        {
            return Start(name, Configuration.DefineWith(properties));
        }

        /// <summary>
        /// Answers a new <c>World</c> with the given <paramref name="name"/> and that is configured with
        /// the contents of the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> name to assign to the new <c>World</c> instance.</param>
        /// <param name="configuration">The <see cref="Vlingo.Actors.Configuration"/> used for configuration</param>
        /// <returns>A <c>World</c> instance.</returns>
        public static World Start(string name, Configuration configuration)
        {
            if (name == null)
            {
                throw new ArgumentException("The world name must not be null.");
            }

            return new World(name, configuration);
        }

        /// <summary>
        /// Answers a new <c>World</c> with the given <paramref name="name"/> and that is configured with
        /// the contents of the default <see cref="Vlingo.Actors.Configuration"/> of sensible settings.
        /// </summary>
        /// <param name="name">The <c>string</c> name to assign to the new <c>World</c> instance.</param>
        /// <returns>A <c>World</c> instance.</returns>
        public static World StartWithDefault(string name)
        {
            return Start(name, Configuration.Define());
        }

        /// <summary>
        /// Answers a new concrete <c>Actor</c> that is defined by the parameters of <paramref name="definition"/>
        /// and supports the protocol defined by <typeparamref name="T"/> protocol.
        /// </summary>
        /// <typeparam name="T">The protocol type.</typeparam>
        /// <param name="definition">The <c>Definition</c> providing parameters to the<c>Actor</c>.</param>
        /// <returns></returns>
        public T ActorFor<T>(Definition definition)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo/actors: Stopped.");
            }

            return Stage.ActorFor<T>(definition);
        }

        /// <summary>
        /// Answers a <c>Protocols</c> that provides one or more supported <paramref name="protocols"/> for the
        /// newly created <c>Actor</c> according to <paramref name="definition"/>.
        /// </summary>
        /// <param name="definition">The <c>Definition</c> providing parameters to the<c>Actor</c>.</param>
        /// <param name="protocols">The array of protocols that the <c>Actor</c> supports.</param>
        /// <returns></returns>
        public Protocols ActorFor(Definition definition, Type[] protocols)
        {
            if (IsTerminated)
            {
                throw new InvalidOperationException("vlingo/actors: Stopped.");
            }

            return Stage.ActorFor(definition, protocols);
        }

        /// <summary>
        /// Gets the dead-letters of this <c>World</c>, which is backed
        /// by an <c>Actor</c>. Interested parties may register for notifications 
        /// as a <see cref="IDeadLettersListener"/> via <see cref="IDeadLetters"/> protocol.
        /// </summary>
        public IDeadLetters DeadLetters { get; internal set; }

        /// <summary>
        /// Answers a new <see cref="ICompletesEventually"/> instance that backs the <paramref name="clientCompletes"/>.
        /// This manages the <c>ICompletes</c> using the <c>CompletesEventually</c> plugin <c>Actor</c> pool.
        /// </summary>
        /// <param name="clientCompletes">The <see cref="ICompletesEventually"/> allocated for eventual completion of <c>clientCompletes</c></param>
        /// <returns></returns>
        public ICompletesEventually CompletesFor(ICompletes clientCompletes)
            => completesProviderKeeper.FindDefault().ProvideCompletesFor(clientCompletes);

        /// <summary>
        /// Gets the default <c>ILogger</c> that is registered with this <c>World</c>. The
        /// <c>ILogger</c> protocol is implemented by an <c>Actor</c> such that all logging is
        /// asynchronous.
        /// </summary>
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

        /// <summary>
        /// Gets the <c>Actor</c> that serves as the default parent for this <c>World</c>.
        /// Unless overridden using <c>Configuration</c> (e.g. <c>Properties</c> or fluent <c>Configuration</c>)s
        /// </summary>
        public Actor DefaultParent { get; private set; }

        /// <summary>
        /// Answers the <c>ISupervisor</c> protocol for sending messages to the default supervisor.
        /// Unless overridden using <c>Configuration</c> (e.g. <c>Properties</c> or fluent <c>Configuration</c>)s
        /// The default supervisor is the single <c>PublicRootActor</c>.
        /// </summary>
        public ISupervisor DefaultSupervisor
        {
            get
            {
                if (defaultSupervisor == null)
                {
                    defaultSupervisor = DefaultParent.SelfAs<ISupervisor>();
                }

                return defaultSupervisor;
            }
        }

        /// <summary>
        /// Answers the <c>ILogger</c> named with <paramref name="name"/>, or <c>null</c> if it does not exist.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the logger.</param>
        /// <returns></returns>
        public ILogger Logger(string name) => loggerProviderKeeper.FindNamed(name).Logger;

        /// <summary>
        /// Gets the <c>string</c> name of this <c>World</c>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Registers the <c>ICompletesEventuallyProvider</c> plugin by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the <c>ICompletesEventuallyProvider</c> to register.</param>
        /// <param name="completesEventuallyProvider">The <c>ICompletesEventuallyProvider</c> to register.</param>
        public void Register(string name, ICompletesEventuallyProvider completesEventuallyProvider)
        {
            completesEventuallyProvider.InitializeUsing(Stage);
            completesProviderKeeper.Keep(name, completesEventuallyProvider);
        }

        /// <summary>
        /// Registers the <c>ILoggerProvider</c> plugin by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> of the logger provider.</param>
        /// <param name="isDefault">The <c>bool</c> value indicating whether this is the default logger provider.</param>
        /// <param name="loggerProvider">The <c>ILoggerProvider</c> to register.</param>
        public void Register(string name, bool isDefault, ILoggerProvider loggerProvider)
        {
            var actualDefault = loggerProviderKeeper.FindDefault() == null ? true : isDefault;
            loggerProviderKeeper.Keep(name, actualDefault, loggerProvider);
            defaultLogger = loggerProviderKeeper.FindDefault().Logger;
        }

        /// <summary>
        /// Registers the <c>IMailboxProvider</c> plugin by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the mailbox provider to register.</param>
        /// <param name="isDefault">The <c>bool</c> value indicating whether this is the default mailbox provider.</param>
        /// <param name="mailboxProvider">The <c>IMailboxProvider</c> to register.</param>
        public void Register(string name, bool isDefault, IMailboxProvider mailboxProvider)
            => mailboxProviderKeeper.Keep(name, isDefault, mailboxProvider);

        /// <summary>
        /// Registers the<paramref name="supervisorClass"/> plugin by <paramref name="name"/> that will supervise all <c>Actor</c> that implement the <paramref name="supervisedProtocol"/>.
        /// </summary>
        /// <param name="stageName">The <c>string</c> of the <c>Stage</c> in which the <paramref name="supervisorClass"/> is be registered.</param>
        /// <param name="name">The <c>string</c> name of the supervisor to register.</param>
        /// <param name="supervisedProtocol">The protocol for which the supervisor will supervise.</param>
        /// <param name="supervisorClass">The <c>Type</c> (which should be a subclass of <c>Actor</c>) to register as a supervisor.</param>
        public void RegisterCommonSupervisor(string stageName, string name, Type supervisedProtocol, Type supervisorClass)
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

        /// <summary>
        /// Registers the <paramref name="supervisorClass"/> plugin by <paramref name="name"/> that will serve as the default supervise for all <c>Actor</c>
        /// that are not supervised by a specific supervisor.
        /// </summary>
        /// <param name="stageName">The <c>string</c> of the <c>Stage</c> in which the <paramref name="supervisorClass"/> is be registered.</param>
        /// <param name="name">The <c>string</c> name of the supervisor to register.</param>
        /// <param name="supervisorClass">The <c>Type</c> (which should be a subclass of <c>Actor</c>) to register as a supervisor.</param>
        public void RegisterDefaultSupervisor(string stageName, string name, Type supervisorClass)
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

        /// <summary>
        /// Registers the <c>ICompletesEventuallyProviderKeeper</c> plugin.
        /// </summary>
        /// <param name="keeper">The <c>ICompletesEventuallyProviderKeeper</c> to register.</param>
        public void RegisterCompletesEventuallyProviderKeeper(ICompletesEventuallyProviderKeeper keeper)
        {
            if (this.completesProviderKeeper != null)
            {
                this.completesProviderKeeper.Close();
            }

            this.completesProviderKeeper = keeper;
        }

        /// <summary>
        /// Registers the <c>ILoggerProviderKeeper</c> plugin.
        /// </summary>
        /// <param name="keeper">The <c>ILoggerProviderKeeper</c> to register.</param>
        public void RegisterLoggerProviderKeeper(ILoggerProviderKeeper keeper)
        {
            if (this.loggerProviderKeeper != null)
            {
                this.loggerProviderKeeper.Close();
            }
            this.loggerProviderKeeper = keeper;
        }

        /// <summary>
        /// Registers the <c>IMailboxProviderKeeper</c> plugin.
        /// </summary>
        /// <param name="keeper">The <c>IMailboxProviderKeeper</c> to register.</param>
        public void RegisterMailboxProviderKeeper(IMailboxProviderKeeper keeper)
        {
            if (this.mailboxProviderKeeper != null)
            {
                this.mailboxProviderKeeper.Close();
            }
            this.mailboxProviderKeeper = keeper;
        }

        /// <summary>
        /// Registers the dynamic dependencies by <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the dynamic dependency.</param>
        /// <param name="dep">The dependency <c>object</c> to register.</param>
        public void RegisterDynamic(string name, object dep)
        {
            this.dynamicDependencies[name] = dep;
        }

        /// <summary>
        /// Answers the <typeparamref name="TDependency"/> instance of the <paramref name="name"/> named dependency.
        /// </summary>
        /// <typeparam name="TDependency">The dependecy type.</typeparam>
        /// <param name="name">The <c>string</c> name of dynamic dependency.</param>
        /// <returns></returns>
        public TDependency ResolveDynamic<TDependency>(string name)
        {
            return (TDependency)this.dynamicDependencies[name];
        }

        /// <summary>
        /// Gets the default <c>Stage</c>, which is the <c>Stage</c> created when this <c>World</c> was started.
        /// </summary>
        public Stage Stage => StageNamed(DefaultStage);

        private readonly object stageNamedMutex = new object();
        /// <summary>
        /// Answers the <c>Stage</c> named by <paramref name="name"/>, or the newly created <c>Stage</c> instance named by <paramref name="name"/>
        /// if the {@code Stage} does not already exist.
        /// </summary>
        /// <param name="name">The <c>string</c> name of the <c>Stage</c> to answer.</param>
        /// <returns></returns>
        public Stage StageNamed(string name)
        {
            lock (stageNamedMutex)
            {
                if (!stages.TryGetValue(name, out Stage stage))
                {
                    stage = new Stage(this, name);
                    if (!string.Equals(name, DefaultStage))
                    {
                        stage.StartDirectoryScanner();
                    }
                    stages[name] = stage;
                }

                return stage;
            }
        }

        /// <summary>
        /// Gets whether or not this <c>World</c> has been terminated or is in the process of termination.
        /// </summary>
        public bool IsTerminated => Stage.IsStopped;

        /// <summary>
        /// Initiates the <c>World</c> terminate process if the process has not already been initiated.
        /// </summary>
        public void Terminate()
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

        /// <summary>
        /// Answers this <c>World</c> instance.
        /// </summary>
        World IRegistrar.World => this;

        /// <summary>
        /// Answers the <c>IMailbox</c> instance by <paramref name="mailboxName"/> and <paramref name="hashCode"/>. (INTERNAL ONLY)
        /// </summary>
        /// <param name="mailboxName">The <c>string</c> name of the <c>IMailbox</c> type to use.</param>
        /// <param name="hashCode">The <c>int</c> hash code to help determine which <c>IMailbox</c> instance to assign.</param>
        /// <returns></returns>
        internal IMailbox AssignMailbox(string mailboxName, int hashCode)
            => mailboxProviderKeeper.AssignMailbox(mailboxName, hashCode);

        /// <summary>
        /// Answers a name for a <c>IMailbox</c> given a <paramref name="candidateMailboxName"/>, which if non-existing
        /// the name of the default mailbox is answered. (INTERNAL ONLY)
        /// </summary>
        /// <param name="candidateMailboxName">the <c>string</c> name of the desired <c>IMailbox</c></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the name of the default mailbox. (INTERNAL ONLY)
        /// </summary>
        /// <returns></returns>
        internal string FindDefaultMailboxName()
        {
            return mailboxProviderKeeper.FindDefault();
        }

        private readonly object defaultParentMutex = new object();
        /// <summary>
        /// Sets the <paramref name="defaultParent"/> <c>Actor</c> as the default for this <c>World</c>. (INTERNAL ONLY)
        /// </summary>
        /// <param name="defaultParent">The <c>Actor</c> to use as default parent.</param>
        internal void SetDefaultParent(Actor defaultParent)
        {
            lock (defaultParentMutex)
            {
                if (defaultParent != null && DefaultParent != null)
                {
                    throw new InvalidOperationException("Default parent already exists.");
                }

                DefaultParent = defaultParent;
            }
        }

        private readonly object deadLettersMutex = new object();
        /// <summary>
        /// Sets the <paramref name="deadLetters"/> as the default for this <c>World</c>. (INTERNAL ONLY)
        /// </summary>
        /// <param name="deadLetters">The <c>IDeadLetters</c> to use as the default.</param>
        internal void SetDeadLetters(IDeadLetters deadLetters)
        {
            lock (deadLettersMutex)
            {
                if (deadLetters != null && DeadLetters != null)
                {
                    deadLetters.Stop();
                    throw new InvalidOperationException("Dead letters already exists.");
                }

                DeadLetters = deadLetters;
            }
        }

        /// <summary>
        /// Gets the <c>PrivateRootActor</c> instance as a <c>IStoppable</c>. (INTERNAL ONLY)
        /// </summary>
        internal IStoppable PrivateRoot { get; private set; }

        private readonly object privateRootMutex = new object();
        /// <summary>
        /// Sets the <c>PrivateRootActor</c> instances as a <c>IStoppable</c>. (INTERNAL ONLY)
        /// </summary>
        /// <param name="privateRoot">The <c>IStoppable</c> protocol backed by the <c>PrivateRootActor</c></param>
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

        /// <summary>
        /// Gets the <c>PublicRootActor</c> instance as a <c>IStoppable</c>. (INTERNAL ONLY)
        /// </summary>
        internal IStoppable PublicRoot { get; private set; }

        public readonly object publicRootMutex = new object();
        /// <summary>
        /// Sets the <c>PublicRootActor</c> instances as a <c>IStoppable</c>. (INTERNAL ONLY)
        /// </summary>
        /// <param name="publicRoot">The <c>IStoppable</c> protocol backed by the <c>PublicRootActor</c></param>
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

        /// <summary>
        /// Starts the <c>PrivateRootActor</c>. When the <c>PrivateRootActor</c> starts it will in turn
        /// start the <c>PublicRootActor</c>.
        /// </summary>
        /// <param name="stage">The <c>Stage</c> in which to start the <c>PrivateRootActor</c>.</param>
        /// <param name="logger">The default <c>ILogger</c> for this <c>World</c> and <c>Stage</c>.</param>
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