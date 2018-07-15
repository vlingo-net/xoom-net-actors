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
    public sealed class ActorProxy
    {
        private static readonly DynaClassLoader classLoader = new DynaClassLoader(typeof(ActorProxy).GetAssemblyLoadContext());
        private static readonly DynaCompiler proxyCompiler = new DynaCompiler();

        public static T CreateFor<T>(Actor actor, IMailbox mailbox)
        {
            var proxyClassName = FullyQualifiedClassNameFor<T>("__Proxy");

            var maybeProxy = actor.LifeCycle.Environment.LookUpProxy<T>();

            if(maybeProxy != null)
            {
                return maybeProxy;
            }

            T newProxy;
            try
            {
                newProxy = TryCreate<T>(actor, mailbox, proxyClassName);
            }
            catch (Exception)
            {
                newProxy = TryGenerateCreate<T>(actor, mailbox, proxyClassName);
            }

            actor.LifeCycle.Environment.CacheProxy<T>(newProxy);

            return newProxy;
        }

        private static T TryCreate<T>(Actor actor, IMailbox mailbox, string targetClassName)
        {
            var proxyClass = classLoader.LoadClass(targetClassName);
            return TryCreateWithProxyClass<T>(proxyClass, actor, mailbox);
        }

        private static T TryCreateWithProxyClass<T>(Type proxyClass, Actor actor, IMailbox mailbox)
            => (T)Activator.CreateInstance(proxyClass, actor, mailbox);

        private static T TryGenerateCreate<T>(Actor actor, IMailbox mailbox, string targetClassName)
        {
            try
            {
                var generator = ProxyGenerator.ForMain(true);
                return TryGenerateCreate<T>(actor, mailbox, generator, targetClassName);
            }
            catch(Exception)
            {
                try
                {
                    var generator = ProxyGenerator.ForTest(true);
                    return TryGenerateCreate<T>(actor, mailbox, generator, targetClassName);
                }
                catch(Exception etest)
                {
                    throw new ArgumentException($"Actor proxy {typeof(T).Name} not created for main or test: {etest.Message}", etest);
                }
            }
        }

        private static T TryGenerateCreate<T>(Actor actor, IMailbox mailbox, ProxyGenerator generator, string targetClassName)
        {
            var protocol = typeof(T);
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
                return TryCreateWithProxyClass<T>(proxyClass, actor, mailbox);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Actor proxy {protocol.Name} not created because: {e.Message}", e);
            }
        }
    }
}
