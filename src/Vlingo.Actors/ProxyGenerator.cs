// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Vlingo.Common.Compiler;

using static Vlingo.Common.Compiler.DynaFile;
using static Vlingo.Common.Compiler.DynaNaming;

namespace Vlingo.Actors
{
    public class ProxyGenerator
    {
        public sealed class Result
        {
            internal Result(
                string fullyQualifiedClassName,
                string className,
                string source,
                FileInfo sourceFile)
            {
                FullyQualifiedClassName = fullyQualifiedClassName;
                ClassName = className;
                Source = source;
                SourceFile = sourceFile;
            }

            public string FullyQualifiedClassName { get; }
            public string ClassName { get; }
            public string Source { get; }
            public FileInfo SourceFile { get; }
        }

        private readonly string rootOfGenerated;
        private readonly bool persist;
        private readonly FileInfo targetClassPath;

        internal DynaType Type { get; }


        public static ProxyGenerator ForMain(bool persist)
        {
            var root = Properties.Instance.GetProperty("proxy.generated.classes.main", RootOfMainClasses);
            return new ProxyGenerator(root, DynaType.Main, persist);
        }

        public static ProxyGenerator ForTest(bool persist)
        {
            var root = Properties.Instance.GetProperty("proxy.generated.classes.test", RootOfTestClasses);
            return new ProxyGenerator(root, DynaType.Test, persist);
        }

        public Result GenerateFor(Type actorProtocol)
        {
            Console.WriteLine("vlingo-net/actors: Generating proxy for " + (Type == DynaType.Main ? "main" : "test") + ": " + actorProtocol.Name);
            try
            {
                var proxyClassSource = ProxyClassSource(actorProtocol);
                var fullyQualifiedClassName = FullyQualifiedClassNameFor(actorProtocol, "__Proxy");
                var relativeTargetFile = ToFullPath(fullyQualifiedClassName);
                var sourceFile = persist ?
                    PersistProxyClassSource(fullyQualifiedClassName, relativeTargetFile, proxyClassSource) :
                    new FileInfo(relativeTargetFile);

                return new Result(fullyQualifiedClassName, ClassNameFor(actorProtocol, "__Proxy"), proxyClassSource, sourceFile);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Cannot generate proxy class for: {actorProtocol.Name}", ex);
            }
        }

        private static string RootOfGeneratedSources(DynaType type)
            => type == DynaType.Main ?
                Properties.Instance.GetProperty("proxy.generated.sources.main", GeneratedSources) :
                Properties.Instance.GetProperty("proxy.generated.sources.test", GeneratedTestSources);

        private ProxyGenerator(string rootOfClasses, DynaType type, bool persist)
        {
            Type = type;
            rootOfGenerated = RootOfGeneratedSources(type);
            this.persist = persist;
            targetClassPath = new FileInfo(rootOfClasses);
        }

        private string ClassStatement(Type protocolInterface)
            => string.Format("public class {0} : {1}\n{{",
                ClassNameFor(protocolInterface, "__Proxy"),
                GetSimpleTypeName(protocolInterface));

        private string Constructor(Type protocolInterface)
        {
            var builder = new StringBuilder();

            var signature = string.Format("  public {0}(Actor actor, IMailbox mailbox)", ClassNameFor(protocolInterface, "__Proxy"));

            builder
                .Append(signature).Append("\n")
                .Append("  {\n")
                .Append("    this.actor = actor;\n")
                .Append("    this.mailbox = mailbox;\n")
                .Append("  }");

            return builder.ToString();
        }

        private string ImportStatements(
            Type protocolInterface,
            IEnumerable<MethodInfo> methods,
            IEnumerable<PropertyInfo> properties)
        {
            var namespaces = new HashSet<string>();
            namespaces.Add("System");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add(typeof(Actor).Namespace);

            namespaces.Add(protocolInterface.Namespace);

            GetAllUsedTypesIn(protocolInterface, methods, properties)
                .ToList()
                .ForEach(t => namespaces.Add(t.Namespace));

            return string.Join('\n', namespaces.Select(x => $"using {x};"));
            
        }

        private string InstanceVariables()
        {
            var builder = new StringBuilder();
            builder
                .Append("  private readonly Actor actor;\n")
                .Append("  private readonly IMailbox mailbox;\n");

            return builder.ToString();
        }

        private IEnumerable<Type> GetAllUsedTypesIn(
            Type type,
            IEnumerable<MethodInfo> methods,
            IEnumerable<PropertyInfo> properties)
        {
            var allReturnTypes = methods
                .Select(m => m.ReturnType)
                .Concat(properties.Select(p => p.PropertyType));

            return GetInnerAndOuterTypes(type)
                .Concat(allReturnTypes)
                .Concat(allReturnTypes.SelectMany(x => GetInnerAndOuterTypes(x)))
                .Distinct();
        }

        private IEnumerable<Type> GetInnerAndOuterTypes(Type type) => GetOuterTypes(type).Concat(GetInnerTypes(type));

        private IEnumerable<Type> GetOuterTypes(Type type)
        {
            var t = type.DeclaringType;
            while (t != null)
            {
                yield return t;
                t = t.DeclaringType;
            }
        }

        private IEnumerable<Type> GetInnerTypes(Type type)
        {
            if (!type.IsGenericType)
            {
                yield break;
            }

            foreach(var t in type.GetGenericArguments())
            {
                yield return t;

                foreach(var outer in GetOuterTypes(t))
                {
                    yield return outer;
                }
                foreach(var inner in GetInnerTypes(t))
                {
                    yield return inner;
                }
            }
        }

        private string RepresentationStatements(IEnumerable<MethodInfo> methods)
        {
            var builder = new StringBuilder();

            int count = 0;

            foreach (var method in methods)
            {
                if (!method.IsStatic)
                {
                    var statement = string.Format("  private const string {0}Representation{1} = \"{0}({2})\";\n",
                                    method.Name,
                                    ++count,
                                    string.Join(", ", method.GetParameters().Select(p => GetSimpleTypeName(p.ParameterType))));

                    builder.Append(statement);
                }
            }

            return builder.ToString();
        }

        private string GetPropertyDefinition(PropertyInfo property)
        {
            var declaration = $"  public {GetSimpleTypeName(property.PropertyType)} {property.Name}";

            if(property.CanRead && property.CanWrite)
            {
                return $"{declaration} {{ get; set; }}\n";
            }

            return $"{declaration} => {DefaultReturnValueString(property.PropertyType)};\n";
        }

        private string PropertyDefinitions(IEnumerable<PropertyInfo> properties)
        {
            var builder = new StringBuilder();
            foreach(var prop in properties)
            {
                builder.Append(GetPropertyDefinition(prop));
            }

            return builder.ToString();
        }

        private string GetMethodDefinition(Type protocolInterface, MethodInfo method, int count)
        {
            var completes = DoesImplementICompletes(method.ReturnType);

            var methodParamSignature = string.Join(", ", method.GetParameters().Select(p => $"{GetSimpleTypeName(p.ParameterType)} {p.Name}"));
            var methodSignature = string.Format("  public {0} {1}({2})",
                GetSimpleTypeName(method.ReturnType),
                method.Name,
                methodParamSignature);

            var ifNotStopped = "    if(!actor.IsStopped)\n    {";
            var consumerStatement = string.Format("      Action<{0}> consumer = actor => actor.{1}({2});",
                GetSimpleTypeName(protocolInterface),
                method.Name,
                string.Join(", ", method.GetParameters().Select(p => p.Name)));
            var completesStatement = completes ? string.Format("      var completes = new BasicCompletes<{0}>(actor.Scheduler);\n", GetSimpleTypeName(method.ReturnType.GetGenericArguments().First())) : "";
            var representationName = string.Format("{0}Representation{1}", method.Name, count);
            var mailboxSendStatement = string.Format("      mailbox.Send(new LocalMessage<{0}>(actor, consumer, {1}{2}));",
                GetSimpleTypeName(protocolInterface),
                completes ? "completes, " : "",
                representationName);
            var completesReturnStatement = completes ? "        return completes;\n" : "";
            var elseDead = string.Format("      actor.DeadLetters.FailedDelivery(new DeadLetter(actor, {0}));", representationName);
            var returnValue = DefaultReturnValueString(method.ReturnType);
            var returnStatement = string.IsNullOrEmpty(returnValue) ? "" : string.Format("    return {0};\n", returnValue);

            var builder = new StringBuilder();
            builder
                .Append(methodSignature).Append("\n")
                .Append("  {\n")
                .Append(ifNotStopped).Append("\n")
                .Append(consumerStatement).Append("\n")
                .Append(completesStatement)
                .Append(mailboxSendStatement).Append("\n")
                .Append(completesReturnStatement)
                .Append("    }\n")
                .Append("    else\n")
                .Append("    {\n")
                .Append(elseDead).Append("\n")
                .Append("    }\n")
                .Append(returnStatement)
                .Append("  }\n");

            return builder.ToString();
        }

        private string MethodDefinitions(Type protocolInterface, IEnumerable<MethodInfo> methods)
        {
            var builder = new StringBuilder();
            int count = 0;
            foreach(var method in methods)
            {
                builder.Append(GetMethodDefinition(protocolInterface, method, ++count));
            }

            return builder.ToString();
        }

        private string NamespaceStatement(Type protocolInterface, bool hasNamespace) => hasNamespace ? $"namespace {protocolInterface.Namespace}\n{{" : string.Empty;

        private FileInfo PersistProxyClassSource(string fullyQualifiedClassName, string relativePathToClass, string proxyClassSource)
        {
            var pathToGeneratedSource = ToNamespacePath(fullyQualifiedClassName);
            var dir = new DirectoryInfo(rootOfGenerated + pathToGeneratedSource);

            if (!dir.Exists)
            {
                dir.Create();
            }

            var pathToSource = rootOfGenerated + relativePathToClass + ".cs";

            return PersistDynaClassSource(pathToSource, proxyClassSource);
        }

        private IEnumerable<MethodInfo> GetAbstractMethodsFor(Type type)
        {
            foreach (var m in type.GetMethods().Where(m => !m.IsSpecialName && m.IsAbstract))
            {
                yield return m;
            }

            foreach(var t in type.GetInterfaces())
            {
                foreach(var m in GetAbstractMethodsFor(t))
                {
                    yield return m;
                }
            }
        }

        private IEnumerable<PropertyInfo> GetPropertiesFor(Type type)
        {
            foreach (var p in type.GetProperties().Where(m => !m.IsSpecialName))
            {
                yield return p;
            }

            foreach (var t in type.GetInterfaces())
            {
                foreach (var p in GetPropertiesFor(t))
                {
                    yield return p;
                }
            }
        }

        private string ProxyClassSource(Type protocolInterface)
        {
            var hasNamespace = !string.IsNullOrWhiteSpace(protocolInterface.Namespace);
            var methods = GetAbstractMethodsFor(protocolInterface).ToList();
            var properties = GetPropertiesFor(protocolInterface).ToList();
            var builder = new StringBuilder();
            builder
                .Append(ImportStatements(protocolInterface, methods, properties)).Append("\n")
                .Append(NamespaceStatement(protocolInterface, hasNamespace)).Append("\n")
                .Append(ClassStatement(protocolInterface)).Append("\n")
                .Append(RepresentationStatements(methods)).Append("\n")
                .Append(InstanceVariables()).Append("\n")
                .Append(Constructor(protocolInterface)).Append("\n")
                .Append(PropertyDefinitions(properties)).Append("\n")
                .Append(MethodDefinitions(protocolInterface, methods)).Append("\n")
                .Append("}\n");
            if (hasNamespace)
            {
                builder.Append("}\n");
            }

            return builder.ToString();
        }

        private string DefaultReturnValueString(Type type)
        {
            if(type == typeof(void))
            {
                return string.Empty;
            }

            if(!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                return "null";
            }

            if(type == typeof(bool))
            {
                return "false";
            }

            if (type.IsEnum)
            {
                return $"{type.Name}.{Activator.CreateInstance(type).ToString()}";
            }

            return Activator.CreateInstance(type).ToString();
        }

        private static readonly IDictionary<Type, string> SimpleTypeNames = new Dictionary<Type, string>
        {
            [typeof(void)] = "void",
            [typeof(object)] = "object",
            [typeof(string)] = "string",
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(char)] = "char",
            [typeof(decimal)] = "decimal",
            [typeof(double)] = "double",
            [typeof(float)] = "float",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort"
        };

        private string GetSimpleTypeName(Type type)
        {
            if(SimpleTypeNames.ContainsKey(type))
            {
                return SimpleTypeNames[type];
            }

            if(Nullable.GetUnderlyingType(type) != null)
            {
                return GetSimpleTypeName(Nullable.GetUnderlyingType(type)) + "?";
            }

            if (type.IsGenericType)
            {
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                return $"{name}<" + string.Join(", ", type.GenericTypeArguments.Select(GetSimpleTypeName)) + ">";
            }

            return type.Name;
        }
        
        private static bool DoesImplementICompletes(Type type)
        {
            Type[] interfaces;
            var completesUnboundedType = typeof(ICompletes<>);

            if (type.IsInterface && type.IsGenericType)
            {
                interfaces = new[] { type };
            }
            else
            {
                interfaces = type.GetInterfaces();
            }

            if (interfaces == null || interfaces.Length == 0)
            {
                return false;
            }

            return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == completesUnboundedType);
        }
    }
}
