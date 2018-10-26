// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;

namespace Vlingo.Actors
{
    internal class ActorFactory
    {
        internal static readonly ThreadLocal<Environment> ThreadLocalEnvironment = new ThreadLocal<Environment>(false);

        internal static Actor ActorFor(
          Stage stage,
          Actor parent,
          Definition definition,
          Address address,
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
                    catch(Exception ex)
                    {
                        logger.Log($"vlingo-net/actors: ActorFactory: failed because: {ex.Message}", ex);
                        Console.WriteLine(ex.StackTrace);
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
            Address address,
            Definition definition)
        {
            var mailboxName = stage.World.MailboxNameFrom(definition.MailboxName);
            var mailbox = stage.World.AssignMailbox(mailboxName, address.GetHashCode());

            return mailbox;
        }
    }
}