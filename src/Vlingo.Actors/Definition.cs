using System;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    // TODO: possible removal/cleanup of overloaded ctors
    public sealed class Definition
    {
        internal static readonly List<object> NoParameters = new List<object>();

        public static Definition Has<T>(List<object> parameters) where T : Actor 
            => new Definition(typeof(T), parameters);

        public static Definition Has<T>(List<object> parameters, Logger logger) where T : Actor
            => new Definition(typeof(T), parameters, logger);

        public static Definition Has<T>(List<object> parameters, string actorName) where T : Actor
            => new Definition(typeof(T), parameters, actorName);

        public static Definition Has<T>(
            List<object> parameters,
            string actorName,
            Logger logger)
            where T : Actor
            => new Definition(typeof(T), parameters, actorName, logger);

        public static Definition Has<T>(
            List<object> parameters,
            Actor parent,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, actorName);

        public static Definition Has<T>(
            List<object> parameters,
            string mailboxName,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, null, mailboxName, actorName);

        public static Definition Has<T>(
            List<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName);

        public static Definition Has<T>(
            List<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName,
            Logger logger)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, logger);

        // TODO: possible cleanup
        public static List<object> Parameters(params object[] parameters)
        {
            return parameters.ToList();
        }

        public Type Type { get; }
        private List<object> parameters;
        public Actor Parent { get; }
        public string MailboxName { get; }
        public string ActorName { get; }
        public ISupervisor Supervisor { get; }
        private Logger Logger { get; }

        private Definition(
            Type type,
            List<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName,
            Logger logger)
        {
            Type = type;
            this.parameters = parameters;
            Parent = parent;
            MailboxName = mailboxName;
            ActorName = actorName;
            Supervisor = AssignSupervisor(parent);
            Logger = logger;
        }

        private Definition(
            Type type,
            List<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName) :
        this(type, parameters, parent, mailboxName, actorName, null)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            Actor parent,
            string actorName) :
        this(type, parameters, parent, null, actorName, null)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            string actorName) :
        this(type, parameters, null, null, actorName, null)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            Actor parent,
            string actorName,
            Logger logger) :
        this(type, parameters, parent, null, actorName, logger)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            string actorName,
            Logger logger) :
        this(type, parameters, null, null, actorName, logger)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            Logger logger) :
        this(type, parameters, null, null, null, logger)
        {
        }

        private Definition(Type type, List<object> parameters) :
        this(type, parameters, null, null, null, null)
        {
        }

        public Logger LoggerOr(Logger defaultLogger)
        {
            return Logger ?? defaultLogger;
        }

        public List<object> Parameters() => new List<object>(InternalParameters());

        public Actor ParentOr(Actor defaultParent) => Parent ?? defaultParent;

        internal List<object> InternalParameters() => parameters;

        private ISupervisor AssignSupervisor(Actor parent)
        {
            if (parent != null && parent is ISupervisor)
            {
                return parent.LifeCycle.Environment.Stage.ActorAs<ISupervisor>(parent);
            }
            return null;
        }
    }
}