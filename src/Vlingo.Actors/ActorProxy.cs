// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Vlingo.Common.Compiler;
using static Vlingo.Common.Compiler.DynaNaming;

namespace Vlingo.Actors
{
    internal static class ActorProxy
    {
        private static readonly object CreateForMutex = new object();

        public static T CreateFor<T>(Actor actor, IMailbox mailbox)
            => (T)CreateFor(typeof(T), actor, mailbox);

        public static object CreateFor(Type protocol, Actor actor, IMailbox mailbox)
        {
            var cacheKeyProtocol = GetProtocolGenericDefinition(protocol);

            var maybeCachedProxy = actor.LifeCycle.Environment.LookUpProxy(cacheKeyProtocol);

            if (maybeCachedProxy != null)
            {
                return maybeCachedProxy;
            }

            object newProxy;

            lock (CreateForMutex)
            {
                var proxyClassnameForLookup = FullyQualifiedClassNameFor(protocol, "__Proxy", true);
                var proxyClassnameForGeneration = FullyQualifiedClassNameFor(protocol, "__Proxy");

                try
                {
                    newProxy = TryCreate(actor, mailbox, protocol, proxyClassnameForLookup);
                }
                catch (Exception e)
                {
                    actor.Logger.Error($"Proxy creation failed because of '{e.Message}' but still trying", e);
                    newProxy = TryGenerateCreate(protocol, actor, mailbox, proxyClassnameForGeneration, proxyClassnameForLookup);
                }

                actor.LifeCycle.Environment.CacheProxy(cacheKeyProtocol, newProxy);
            }

            return newProxy;
        }

        private static Type GetProtocolGenericDefinition(Type protocol)
            => protocol.IsGenericType ? protocol.GetGenericTypeDefinition() : protocol;

        private static Type? LoadProxyClassFor(string targetClassname, Actor actor)
            => ClassLoaderFor(actor).LoadClass(targetClassname);

        private static object TryCreate(Actor actor, IMailbox mailbox, Type protocol, string targetClassname)
        {
            var proxyClass = LoadProxyClassFor(targetClassname, actor);
            if (proxyClass != null && proxyClass.IsGenericTypeDefinition)
            {
                var genericTypeParams = protocol.GetGenericArguments();
                proxyClass = proxyClass.MakeGenericType(genericTypeParams);
            }

            return TryCreateWithProxyClass(proxyClass!, actor, mailbox);
        }

        private static object TryCreateWithProxyClass(Type proxyClass, Actor actor, IMailbox mailbox)
        {
            var instance = Activator.CreateInstance(proxyClass, actor, mailbox);
            if (instance == null)
            {
                throw new ArgumentException($"Cannot create an instance for proxy class '{proxyClass.FullName}'");
            }
            
            return instance;
        }

        private static object TryGenerateCreate(Type protocol, Actor actor, IMailbox mailbox, string targetClassName, string lookupTypeName)
        {
            try
            {
                var generator = ProxyGenerator.ForMain(true, actor.Logger);
                return TryGenerateCreate(protocol, actor, mailbox, generator, targetClassName, lookupTypeName);
            }
            catch (Exception e)
            {
                actor.Logger.Error($"Trying generate proxy but it failed because of '{e.Message}' but still trying", e);
                try
                {
                    var generator = ProxyGenerator.ForTest(true, actor.Logger);
                    return TryGenerateCreate(protocol, actor, mailbox, generator, targetClassName, lookupTypeName);
                }
                catch (Exception etest)
                {
                    throw new ArgumentException($"Actor proxy {protocol.Name} not created for main or test: {etest.Message}", etest);
                }
            }
        }

        private static object TryGenerateCreate(Type protocol, Actor actor, IMailbox mailbox, ProxyGenerator generator, string targetClassName, string lookupTypeName)
        {
            try
            {
                var originalProtocol = protocol;

                if(protocol.IsGenericType && !protocol.IsGenericTypeDefinition)
                {
                    protocol = protocol.GetGenericTypeDefinition();
                }

                // Allows to determine if async/await keyword was used
                var asyncAwaitedMethods = actor.GetType().GetTypeInfo().ImplementedInterfaces
                    .Select(ii => actor.GetType().GetInterfaceMap(ii))
                    .Where(im => im.InterfaceType == originalProtocol)
                    .SelectMany(im => im.TargetMethods)
                    .Where(IsAsyncStateMachine)
                    .ToArray();
                var result = generator.GenerateFor(protocol, asyncAwaitedMethods);
                var input = new Input(
                    protocol,
                    targetClassName,
                    lookupTypeName,
                    result.Source,
                    result.SourceFile,
                    ClassLoaderFor(actor),
                    generator.Type,
                    true);

                var proxyCompiler = new DynaCompiler();
                var proxyClass = proxyCompiler.Compile(input);
                if (proxyClass != null && proxyClass.IsGenericTypeDefinition)
                {
                    proxyClass = proxyClass.MakeGenericType(originalProtocol.GetGenericArguments());
                }
                
                return TryCreateWithProxyClass(proxyClass!, actor, mailbox);
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
        
        private static bool IsAsyncStateMachine(MethodInfo methodInfo)
        {
            var attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            if (methodInfo != null)
            {
                return methodInfo.GetCustomAttribute(attType) is AsyncStateMachineAttribute;
            }

            return false;
        }
    }
}
