// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Actors
{
    internal static class ActorFactory
    {
        internal static readonly ThreadLocal<Environment> ThreadLocalEnvironment = new ThreadLocal<Environment>(false);

        public static Type ActorClassWithProtocol<TProtocol>(string actorClassname) where TProtocol : Actor
            => ActorClassWithProtocol(actorClassname, typeof(TProtocol));

        public static Type ActorClassWithProtocol(string actorClassname, Type protocolClass)
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
          Actor parent,
          Definition definition,
          IAddress address,
          IMailbox mailbox,
          ISupervisor supervisor,
          ILogger logger)
        {
            var environment = new Environment(
                stage,
                address,
                definition,
                parent,
                mailbox,
                supervisor,
                logger);

            ThreadLocalEnvironment.Value = environment;

            Actor actor = null;

            int definitionParameterCount = definition.InternalParameters().Count;

            if (definitionParameterCount == 0)
            {
                actor = (Actor)Activator.CreateInstance(definition.Type);
            }
            else
            {
                foreach (var ctor in definition.Type.GetConstructors())
                {
                    if (ctor.GetParameters().Length != definitionParameterCount)
                        continue;

                    try
                    {
                        actor = (Actor)ctor.Invoke(definition.InternalParameters().ToArray());
                        actor.LifeCycle.SendStart(actor);
                    }
                    catch (Exception ex)
                    {
                        var cause = ex.InnerException ?? ex;
                        logger.Log("ActorFactory: failed actor creation. "
                                    + "This is sometimes cause be the constructor parameter types not matching "
                                    + "the types in the Definition.parameters(). Often it is caused by a "
                                    + "failure in the actor constructor. We have attempted to uncover "
                                    + "the root cause here, but that may not be available in some cases.\n"
                                    + "The root cause may be: " + cause.Message + "\n"
                                    + "See stacktrace for more information. We strongly recommend reviewing your "
                                    + "constructor for possible failures in dependencies that it creates.",
                                    cause);
                    }
                    break;
                }
            }

            if (actor == null)
            {
                throw new ArgumentException("No constructor matches the given number of parameters.");
            }

            if (parent != null)
            {
                parent.LifeCycle.Environment.AddChild(actor);
            }

            return actor;
        }

        internal static IMailbox ActorMailbox(
            Stage stage,
            IAddress address,
            Definition definition)
        {
            var mailboxName = stage.World.MailboxNameFrom(definition.MailboxName);
            var mailbox = stage.World.AssignMailbox(mailboxName, address.GetHashCode());

            return mailbox;
        }
    }
}