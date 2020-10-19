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
        private readonly IEnumerable<object> _parameters;
        private ILogger? Logger { get; }
        
        public static readonly List<object> NoParameters = new List<object>();

        public static Definition From<T>(Stage stage, SerializationProxy<T>? proxy, ILogger logger)
        {
            Actor? parent = null;
            if (proxy?.Parent != null)
            {
                ActorProxyBase<T>.Thunk(stage, proxy.Parent);
                parent = stage.Directory.ActorOf(proxy.Parent?.Address);
            }

            return new Definition(
                proxy?.Type,
                proxy?.Parameters ?? Enumerable.Empty<object>(),
                parent,
                proxy?.MailboxName,
                proxy?.ActorName,
                logger,
                proxy?.Evictable ?? false
            );
        }

        public static Definition Has<T>(IEnumerable<object> parameters) where T : Actor 
            => new Definition(typeof(T), parameters, false);

        public static Definition Has<T>(IEnumerable<object> parameters, ILogger logger) where T : Actor
            => new Definition(typeof(T), parameters, logger);

        public static Definition Has<T>(IEnumerable<object> parameters, string actorName) where T : Actor
            => new Definition(typeof(T), parameters, actorName);

        public static Definition Has(Type typeOfT, IEnumerable<object> parameters)
            => new Definition(typeOfT, parameters);

        public static Definition Has(Type? typeOfT, IEnumerable<object> parameters, string actorName)
            => new Definition(typeOfT, parameters, actorName);
        
        public static Definition Has<T>(Expression<Func<T>> factory, bool evictable)
            => Has(factory, null, null, null, null, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory)
            => Has(factory, null, null, null, null, false);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName, bool evictable)
            => Has(factory, null, null, actorName, null, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName)
            => Has(factory, null, null, actorName, null, false);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName, ILogger? logger, bool evictable)
            => Has(factory, null, null, actorName, logger, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? actorName, ILogger? logger)
            => Has(factory, null, null, actorName, logger, false);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? actorName, bool evictable)
            => Has(factory, parent, null, actorName, null, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? actorName)
            => Has(factory, parent, null, actorName, null, false);

        public static Definition Has<T>(Expression<Func<T>> factory, string? mailboxName, string? actorName, bool evictable)
            => Has(factory, null, mailboxName, actorName, null, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory, string? mailboxName, string? actorName)
            => Has(factory, null, mailboxName, actorName, null, false);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? mailboxName, string? actorName, bool evictable)
            => Has(factory, parent, mailboxName, actorName, null, evictable);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? mailboxName, string? actorName)
            => Has(factory, parent, mailboxName, actorName, null, false);
        
        public static Definition Has<T>(Expression<Func<T>> factory, Actor? parent, string? mailboxName, string? actorName, ILogger? logger, bool evictable)
        {
            var newExpression = factory.Body as NewExpression;
            if (newExpression == null)
                throw new ArgumentException("The create function must be a 'new T (parameters)' expression");

            var expressionType = newExpression.Type;
            if (!typeof(Actor).IsAssignableFrom(expressionType))
                throw new ArgumentException($"The type '{expressionType.FullName}' must be instance of an actor. Derive it from Actor class.");

            var parameters = newExpression.GetArguments();

            return new Definition(expressionType, parameters, parent, mailboxName, actorName, logger, evictable);
        }

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            string actorName,
            ILogger logger,
            bool evictable)
            where T : Actor
            => new Definition(typeof(T), parameters, actorName, logger, evictable);

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            string actorName,
            ILogger logger)
            where T : Actor
            => new Definition(typeof(T), parameters, actorName, logger, false);

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string actorName,
            bool evictable)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, actorName, evictable);
        
        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, actorName, false);

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            string mailboxName,
            string actorName,
            bool evictable)
            where T : Actor
            => new Definition(typeof(T), parameters, null, mailboxName, actorName, evictable);
        
        public static Definition Has<T>(
            IEnumerable<object> parameters,
            string mailboxName,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, null, mailboxName, actorName, false);

        public static Definition Has(
            Type? typeOfT,
            IEnumerable<object> parameters,
            string mailboxName,
            string? actorName,
            bool evictable)
            => new Definition(typeOfT, parameters, null, mailboxName, actorName, evictable);
        
        public static Definition Has(
            Type? typeOfT,
            IEnumerable<object> parameters,
            string mailboxName,
            string? actorName)
            => new Definition(typeOfT, parameters, null, mailboxName, actorName, false);

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName,
            bool evictable)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, evictable);
        
        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, false);

        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName,
            ILogger logger,
            bool evictable)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, logger, evictable);
        
        public static Definition Has<T>(
            IEnumerable<object> parameters,
            Actor parent,
            string mailboxName,
            string actorName,
            ILogger logger)
            where T : Actor
            => new Definition(typeof(T), parameters, parent, mailboxName, actorName, logger, false);

        // TODO: possible cleanup
        public static IEnumerable<object> Parameters(params object[] parameters) => parameters;

        public Type? Type { get; }
        public Actor? Parent { get; }
        public string? MailboxName { get; }
        public string? ActorName { get; }
        public ISupervisor? Supervisor { get; }
        public bool Evictable { get; }

        private Definition(
            Type? type,
            IEnumerable<object> parameters,
            Actor? parent,
            string? mailboxName,
            string? actorName,
            ILogger? logger,
            bool evictable)
        {
            Type = type;
            _parameters = parameters;
            Parent = parent;
            MailboxName = mailboxName;
            ActorName = actorName;
            Supervisor = AssignSupervisor(parent);
            Logger = logger;
            Evictable = evictable;
        }

        private Definition(
            Type? type,
            IEnumerable<object> parameters,
            Actor? parent,
            string mailboxName,
            string? actorName,
            bool evictable) :
            this(type, parameters, parent, mailboxName, actorName, null, evictable)
        {
        }
        
        private Definition(
            Type? type,
            IEnumerable<object> parameters,
            Actor? parent,
            string mailboxName,
            string? actorName) :
        this(type, parameters, parent, mailboxName, actorName, null, false)
        {
        }

        private Definition(
            Type type,
            IEnumerable<object> parameters,
            Actor? parent,
            string actorName,
            bool evictable) :
            this(type, parameters, parent, null, actorName, null, evictable)
        {
        }

        private Definition(
            Type type,
            IEnumerable<object> parameters,
            Actor? parent,
            string actorName) :
        this(type, parameters, parent, null, actorName, null, false)
        {
        }
        
        private Definition(
            Type? type,
            IEnumerable<object> parameters,
            string actorName,
            bool evictable) :
            this(type, parameters, null, null, actorName, null, evictable)
        {
        }

        private Definition(
            Type? type,
            IEnumerable<object> parameters,
            string actorName) :
        this(type, parameters, null, null, actorName, null, false)
        {
        }
        
        private Definition(
            Type type,
            IEnumerable<object> parameters,
            string actorName,
            ILogger logger,
            bool evictable) :
            this(type, parameters, null, null, actorName, logger, evictable)
        {
        }

        private Definition(
            Type type,
            IEnumerable<object> parameters,
            string actorName,
            ILogger logger) :
        this(type, parameters, null, null, actorName, logger, false)
        {
        }
        
        private Definition(
            Type type,
            IEnumerable<object> parameters,
            ILogger logger,
            bool evictable) :
            this(type, parameters, null, null, null, logger, evictable)
        {
        }

        private Definition(
            Type type,
            IEnumerable<object> parameters,
            ILogger logger) :
        this(type, parameters, null, null, null, logger, false)
        {
        }
        
        public Definition(Type type, IEnumerable<object> parameters, bool evictable) :
            this(type, parameters, null, null, null, null, evictable)
        {
        }

        private Definition(Type type, IEnumerable<object> parameters) :
            this(type, parameters, null, null, null, null, false)
        {
        }

        public ILogger LoggerOr(ILogger defaultLogger)
        {
            return Logger ?? defaultLogger;
        }

        public IEnumerable<object> Parameters() => new List<object>(InternalParameters());

        public Actor? ParentOr(Actor? defaultParent) => Parent ?? defaultParent;

        internal IEnumerable<object> InternalParameters() => _parameters;

        private static ISupervisor? AssignSupervisor(Actor? parent)
        {
            if (parent is ISupervisor)
            {
                return parent.LifeCycle.Environment.Stage.ActorAs<ISupervisor>(parent);
            }
            return null;
        }

        public class SerializationProxy<T>
        {
            public string ActorName { get; }
            public string MailboxName { get; }
            public IEnumerable<object> Parameters { get; }
            public ActorProxyStub<T> Parent { get; }
            public Type Type { get; }
            public bool Evictable { get; }

            public static SerializationProxy<T> From(Definition definition) =>
                new SerializationProxy<T>(
                    definition.ActorName!,
                    definition.MailboxName!,
                    definition._parameters!,
                    definition.Parent != null ? new ActorProxyStub<T>(definition.Parent) : null!,
                    definition.Type!,
                    definition.Evictable);

            public SerializationProxy(
                string actorName,
                string mailboxName,
                IEnumerable<object> parameters,
                ActorProxyStub<T> parent,
                Type type,
                bool evictable)
            {
                ActorName = actorName;
                MailboxName = mailboxName;
                Parameters = parameters;
                Parent = parent;
                Type = type;
                Evictable = evictable;
            }

            public override bool Equals(object? obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                
                var that = (SerializationProxy<T>) obj;

                return ActorName == that.ActorName
                       && MailboxName == that.MailboxName
                       && Equals(Parameters, that.Parameters)
                       && Parent == that.Parent
                       && Type == that.Type;
            }

            public override int GetHashCode() =>
                31 * ActorName.GetHashCode() + MailboxName.GetHashCode() + GetHashCode(Parameters) +
                Parent.GetHashCode() + Type.GetHashCode();

            public override string ToString() => $"Definition(ActorName='{ActorName}', MailboxName='{MailboxName}', Parameters='{Parameters.Select(p => p.ToString())}', Parent='{Parent}', type='{Type}')";

            private bool Equals(IEnumerable<object>? p1, IEnumerable<object>? p2)
            {
                if (p1 == null || p2 == null)
                {
                    return false;
                }
                
                var parameters1 = p1.ToList();
                var parameters2 = p2.ToList();

                var count1 = parameters1.Count;
                var count2 = parameters2.Count;
                if (count1 != count2)
                {
                    return false;
                }

                for (var i = 0; i < count1; i++)
                {
                    if (!parameters1[i].Equals(parameters2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private int GetHashCode(IEnumerable<object> parameters)
            {
                var hashCode = 0;
                foreach (var parameter in parameters)
                {
                    if (hashCode == 0)
                    {
                        hashCode += 31 * parameter.GetHashCode();
                    }

                    hashCode += parameter.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}