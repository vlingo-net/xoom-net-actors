// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common.Compiler;
using static Vlingo.Common.Compiler.DynaNaming;

namespace Vlingo.Actors
{
    internal static class ActorProxy
    {
        private static readonly DynaClassLoader classLoader = new DynaClassLoader();
        private static readonly DynaCompiler proxyCompiler = new DynaCompiler();
        private static readonly object _createForMutex = new object();

        public static T CreateFor<T>(Actor actor, IMailbox mailbox)
            => (T)CreateFor(typeof(T), actor, mailbox);

        public static object CreateFor(Type protocol, Actor actor, IMailbox mailbox)
        {
            lock (_createForMutex)
            {
                var proxyClassName = FullyQualifiedClassNameFor(protocol, "__Proxy");

                var maybeProxy = actor.LifeCycle.Environment.LookUpProxy(protocol);

                if (maybeProxy != null)
                {
                    return maybeProxy;
                }

                object newProxy;
                try
                {
                    newProxy = TryCreate(actor, mailbox, proxyClassName);
                }
                catch (Exception)
                {
                    newProxy = TryGenerateCreate(protocol, actor, mailbox, proxyClassName);
                }

                actor.LifeCycle.Environment.CacheProxy(protocol, newProxy);

                return newProxy; 
            }
        }

        private static object TryCreate(Actor actor, IMailbox mailbox, string targetClassName)
        {
            var proxyClass = classLoader.LoadClass(targetClassName);
            return TryCreateWithProxyClass(proxyClass, actor, mailbox);
        }

        private static object TryCreateWithProxyClass(Type proxyClass, Actor actor, IMailbox mailbox)
            => Activator.CreateInstance(proxyClass, actor, mailbox);

        private static object TryGenerateCreate(Type protocol, Actor actor, IMailbox mailbox, string targetClassName)
        {
            try
            {
                var generator = ProxyGenerator.ForMain(true);
                return TryGenerateCreate(protocol, actor, mailbox, generator, targetClassName);
            }
            catch (Exception)
            {
                try
                {
                    var generator = ProxyGenerator.ForTest(true);
                    return TryGenerateCreate(protocol, actor, mailbox, generator, targetClassName);
                }
                catch (Exception etest)
                {
                    throw new ArgumentException($"Actor proxy {protocol.Name} not created for main or test: {etest.Message}", etest);
                }
            }
        }

        private static object TryGenerateCreate(Type protocol, Actor actor, IMailbox mailbox, ProxyGenerator generator, string targetClassName)
        {
            try
            {
                var result = generator.GenerateFor(protocol);
                var input = new Input(
                    protocol,
                    targetClassName,
                    result.Source,
                    result.SourceFile,
                    classLoader,
                    generator.Type,
                    true);

                var proxyClass = proxyCompiler.Compile(input);
                return TryCreateWithProxyClass(proxyClass, actor, mailbox);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Actor proxy {protocol.Name} not created because: {e.Message}", e);
            }
        }
    }
}
