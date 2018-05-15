using System;
using System.Threading;

namespace Vlingo.Actors
{
    public class ActorFactory
    {
        internal static readonly ThreadLocal<Environment> ThreadLocalEnvironment = new ThreadLocal<Environment>(false);

        internal static Actor ActorFor(
          Stage stage,
          Actor parent,
          Definition definition,
          Address address,
          IMailbox mailbox,
          ISupervisor supervisor,
          Logger logger)
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
                        Logger.Log($"vlingo-dotnet/actors: ActorFactory: failed because: {ex.Message}", ex);
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