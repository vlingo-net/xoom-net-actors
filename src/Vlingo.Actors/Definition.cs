// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vlingo.Actors
{
    // TODO: possible removal/cleanup of overloaded ctors
    public sealed class Definition
    {
        private readonly List<object> _parameters;
        private ILogger? Logger { get; }
        
        public static readonly List<object> NoParameters = new List<object>();

        public static Definition Has<T>(List<object> parameters) where T : Actor 
            => new Definition(typeof(T), parameters);

        public static Definition Has<T>(List<object> parameters, ILogger logger) where T : Actor
            => new Definition(typeof(T), parameters, logger);

        public static Definition Has<T>(List<object> parameters, string actorName) where T : Actor
            => new Definition(typeof(T), parameters, actorName);

        public static Definition Has(Type typeOfT, List<object> parameters)
            => new Definition(typeOfT, parameters);

        public static Definition Has(Type? typeOfT, List<object> parameters, string actorName)
            => new Definition(typeOfT, parameters, actorName);
        
        public static Definition Has<T>(Expression<Func<T>> factory)
            => Has(factory, null, null, null, null);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName)
            => Has(factory, null, null, actorName, null);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName, ILogger? logger)
            => Has(factory, null, null, actorName, logger);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? actorName)
            => Has(factory, parent, null, actorName, null);

        public static Definition Has<T>(Expression<Func<T>> factory, string? mailboxName, string? actorName)
            => Has(factory, null, mailboxName, actorName, null);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? mailboxName, string? actorName)
            => Has(factory, parent, mailboxName, actorName, null);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? mailboxName, string? actorName, ILogger? logger)
        {
            var newExpression = factory.Body as NewExpression;
            if (newExpression == null)
                throw new ArgumentException("The create function must be a 'new T (parameters)' expression");

            var expressionType = newExpression.Type;
            if (!typeof(Actor).IsAssignableFrom(expressionType))
                throw new ArgumentException($"The type '{expressionType.FullName}' must be instance of an actor. Derive it from Actor class.");

            var parameters = newExpression.GetArguments().ToList();

            return new Definition(expressionType, parameters, parent, mailboxName, actorName, logger);
        }

        public static Definition Has<T>(
            List<object> parameters,
            string actorName,
            ILogger logger)
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

        public static Definition Has(
            Type? typeOfT,
            List<object> parameters,
            string mailboxName,
            string? actorName)
            => new Definition(typeOfT, parameters, null, mailboxName, actorName);

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
            ILogger logger)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, logger);

        // TODO: possible cleanup
        public static List<object> Parameters(params object[] parameters)
        {
            return parameters.ToList();
        }

        public Type? Type { get; }
        public Actor? Parent { get; }
        public string? MailboxName { get; }
        public string? ActorName { get; }
        public ISupervisor? Supervisor { get; }

        private Definition(
            Type? type,
            List<object> parameters,
            Actor? parent,
            string? mailboxName,
            string? actorName,
            ILogger? logger)
        {
            Type = type;
            _parameters = parameters;
            Parent = parent;
            MailboxName = mailboxName;
            ActorName = actorName;
            Supervisor = AssignSupervisor(parent);
            Logger = logger;
        }

        private Definition(
            Type? type,
            List<object> parameters,
            Actor? parent,
            string mailboxName,
            string? actorName) :
        this(type, parameters, parent, mailboxName, actorName, null)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            Actor? parent,
            string actorName) :
        this(type, parameters, parent, null, actorName, null)
        {
        }

        private Definition(
            Type? type,
            List<object> parameters,
            string actorName) :
        this(type, parameters, null, null, actorName, null)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            string actorName,
            ILogger logger) :
        this(type, parameters, null, null, actorName, logger)
        {
        }

        private Definition(
            Type type,
            List<object> parameters,
            ILogger logger) :
        this(type, parameters, null, null, null, logger)
        {
        }

        private Definition(Type type, List<object> parameters) :
        this(type, parameters, null, null, null, null)
        {
        }

        public ILogger LoggerOr(ILogger defaultLogger)
        {
            return Logger ?? defaultLogger;
        }

        public List<object> Parameters() => new List<object>(InternalParameters());

        public Actor? ParentOr(Actor? defaultParent) => Parent ?? defaultParent;

        internal List<object> InternalParameters() => _parameters;

        private ISupervisor? AssignSupervisor(Actor? parent)
        {
            if (parent != null && parent is ISupervisor)
            {
                return parent.LifeCycle.Environment.Stage.ActorAs<ISupervisor>(parent);
            }
            return null;
        }
    }
}