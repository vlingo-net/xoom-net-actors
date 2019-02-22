// Copyright (c) 2012-2019 Vaughn Vernon. All rights reserved.
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
        private static readonly object _createForMutex = new object();

        public static T CreateFor<T>(Actor actor, IMailbox mailbox)
            => (T)CreateFor(typeof(T), actor, mailbox);

        public static object CreateFor(Type protocol, Actor actor, IMailbox mailbox)
        {
            var maybeCachedProxy  = actor.LifeCycle.Environment.LookUpProxy(protocol);

            if (maybeCachedProxy  != null)
            {
                return maybeCachedProxy ;
            }

            var proxyClassname = FullyQualifiedClassNameFor(protocol, "__Proxy");

            object newProxy;
            try
            {
                newProxy = TryCreate(actor, mailbox, proxyClassname);
            }
            catch (Exception)
            {
                lock (_createForMutex)
                {
                    newProxy = TryGenerateCreate(protocol, actor, mailbox, proxyClassname);
                }
            }

            actor.LifeCycle.Environment.CacheProxy(protocol, newProxy);

            return newProxy; 
        }

        private static Type LoadProxyClassFor(string targetClassname, Actor actor)
            => ClassLoaderFor(actor).LoadClass(targetClassname);
        
        private static object TryCreate(Actor actor, IMailbox mailbox, string targetClassname)
        {
            var proxyClass = LoadProxyClassFor(targetClassname, actor);
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
                    ClassLoaderFor(actor),
                    generator.Type,
                    true);

                var proxyCompiler = new DynaCompiler();
                var proxyClass = proxyCompiler.Compile(input);
                return TryCreateWithProxyClass(proxyClass, actor, mailbox);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Actor proxy {protocol.Name} not created because: {e.Message}", e);
            }
        }

        private static DynaClassLoader ClassLoaderFor(Actor actor)
        {
            var classLoader = actor.LifeCycle.Environment.Stage.World.ClassLoader;
            if (classLoader == null)
            {
                classLoader = new DynaClassLoader();
                actor.Stage.World.ClassLoader = classLoader;
            }

            return classLoader;
        }
    }
}
