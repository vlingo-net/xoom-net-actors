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
        private static readonly DynaCompiler proxyCompiler = new DynaCompiler();
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

            var maybeProxy = TryProxyFor(proxyClassname, actor, mailbox);

            if (maybeProxy != null)
            {
                actor.LifeCycle.Environment.CacheProxy(maybeProxy);
                return maybeProxy;
            }
            
            lock (_createForMutex)
            {   
                object newProxy;
                try
                {
                    newProxy = TryCreate(actor, mailbox, proxyClassname);
                }
                catch (Exception)
                {
                    newProxy = TryGenerateCreate(protocol, actor, mailbox, proxyClassname);
                }

                actor.LifeCycle.Environment.CacheProxy(protocol, newProxy);

                return newProxy; 
            }
        }

        private static Type LoadProxyClassFor(Actor actor, string targetClassname)
            => ClassLoaderFor(actor).LoadClass(targetClassname);
        
        private static object TryCreate(Actor actor, IMailbox mailbox, string targetClassname)
        {
            var proxyClass = LoadProxyClassFor(actor, targetClassname);
            return TryCreateWithProxyClass(proxyClass, actor, mailbox);
        }

        private static object TryProxyFor(string targetClassname, Actor actor, IMailbox mailbox)
        {
            try
            {
                var maybeProxyClass = ClassLoaderFor(actor).LoadClass(targetClassname);
                if (maybeProxyClass != null)
                {
                    return TryCreateWithProxyClass(maybeProxyClass, actor, mailbox);
                }
            }
            catch
            {
                // fall through
            }

            return null;
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
