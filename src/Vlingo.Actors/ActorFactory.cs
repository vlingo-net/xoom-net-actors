// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Vlingo.Actors
{
    public static class ActorFactory
    {
        internal static readonly ThreadLocal<Environment?> ThreadLocalEnvironment = new ThreadLocal<Environment?>(false);

        public static Type ActorClassWithProtocol<TProtocol>(string actorClassname) where TProtocol : Actor
            => ActorClassWithProtocol(actorClassname, typeof(TProtocol));

        private static Type ActorClassWithProtocol(string actorClassname, Type protocolClass)
        {
            try
            {
                var actorClass = Type.GetType(actorClassname);
                AssertActorWithProtocol(actorClass, protocolClass);
                return actorClass;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"The class {actorClassname} cannot be loaded because: {e.Message}", e);
            }
        }

        private static void AssertActorWithProtocol(Type candidateActorClass, Type protocolClass)
        {
            var superclass = candidateActorClass.BaseType;
            while (superclass != null)
            {
                if (superclass == typeof(Actor))
                {
                    break;
                }
                superclass = superclass.BaseType;
            }

            if (superclass == null)
            {
                throw new ArgumentException($"Class must extend Vlingo.Actors.Actor: {candidateActorClass.FullName}");
            }

            foreach (var protocolInterfaceClass in candidateActorClass.GetInterfaces())
            {
                if (protocolClass == protocolInterfaceClass)
                {
                    return;
                }
            }
            throw new ArgumentException($"Actor class {candidateActorClass.FullName} must implement: {protocolClass.FullName}");
        }

        internal static Actor ActorFor(
          Stage stage,
          Actor? parent,
          Definition definition,
          IAddress address,
          IMailbox mailbox,
          ISupervisor? supervisor,
          ILogger logger)
        {
            var environment = new Environment(
                stage,
                address,
                definition,
                parent,
                mailbox,
                supervisor,
                new ActorLoggerAdapter(logger, address, definition.Type!));

            ThreadLocalEnvironment.Value = environment;

            Actor? actor = null;

            var definitionParameterCount = definition.InternalParameters().Count();

            if (definitionParameterCount == 0)
            {
                actor = (Actor)Activator.CreateInstance(definition.Type);
                actor.LifeCycle.SendStart(actor);
            }
            else
            {
                foreach (var ctor in definition.Type!.GetConstructors())
                {
                    if (ctor.GetParameters().Length != definitionParameterCount)
                        continue;

                    actor = Start(ctor, address, definition, logger);

                    if(actor != null)
                    {
                        break;
                    }
                }
            }

            if (actor == null)
            {
                throw new MissingMethodException("No constructor matches the given number of parameters.");
            }

            if (parent != null)
            {
                parent.LifeCycle.Environment.AddChild(actor);
            }

            return actor;
        }

        private static Actor? Start(ConstructorInfo ctor, IAddress address, Definition definition, ILogger logger)
        {
            Actor? actor;
            object[]? args = null;
            Exception? cause = null;

            for (var times = 1; times <= 2; ++times)
            {
                try
                {
                    if (times == 1)
                    {
                        args = definition.InternalParameters().ToArray();
                    }


                    actor = (Actor)ctor.Invoke(definition.InternalParameters().ToArray());
                    actor.LifeCycle.SendStart(actor);
                    return actor;
                }
                catch (Exception ex)
                {
                    cause = ex.InnerException ?? ex;
                    if(times == 1)
                    {
                        args = Unfold(args!);
                    }
                }
            }

            if (cause != null)
            {
                logger.Error("ActorFactory: failed actor creation. "
                                + "This is sometimes cause by the constructor parameter types not matching "
                                + "the types in the Definition.parameters(). Often it is caused by a "
                                + "failure in the actor constructor. We have attempted to uncover "
                                + "the root cause here, but that may not be available in some cases.\n"
                                + "The root cause may be: " + cause.Message + "\n"
                                + "See stacktrace for more information. We strongly recommend reviewing your "
                                + "constructor for possible failures in dependencies that it creates.",
                                cause);

                throw new ArgumentException($"ActorFactory failed actor creation for: {address}");
            }

            return null;
        }

        private static object[] Unfold(object[] args)
        {
            var unfolded = new object[args.Length];
            for (var i = 0; i < args.Length; ++i)
            {
                var currentArg = args[i];
                if (currentArg.GetType().IsArray)
                {
                    unfolded[i] = ((object[])currentArg)[0];
                }
                else
                {
                    unfolded[i] = args[i];
                }
            }

            return unfolded;
        }

        internal static IMailbox ActorMailbox(
            Stage stage,
            IAddress? address,
            Definition definition,
            IMailboxWrapper wrapper)
        {
            var mailboxName = stage.World.MailboxNameFrom(definition.MailboxName);
            var mailbox = stage.World.AssignMailbox(mailboxName, address?.GetHashCode());

            return wrapper.Wrap(address, mailbox);
        }

        internal static IMailbox ActorMailbox(Stage stage,
            IAddress address,
            Definition definition) => ActorMailbox(stage, address, definition, new IdentityMailboxWrapper());
        
        public interface IMailboxWrapper
        {
            IMailbox Wrap(IAddress? address, IMailbox mailbox);
        }
        
        internal class IdentityMailboxWrapper : IMailboxWrapper
        {
            public IMailbox Wrap(IAddress? address, IMailbox mailbox) => mailbox;
        }
    }
}