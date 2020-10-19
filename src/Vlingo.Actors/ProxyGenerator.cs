// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vlingo.Common;
using Vlingo.Common.Compiler;

using static Vlingo.Common.Compiler.DynaFile;
using static Vlingo.Common.Compiler.DynaNaming;

namespace Vlingo.Actors
{
    internal class ProxyGenerator
    {
        internal sealed class Result
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

        private readonly ILogger _logger;
        private readonly bool _persist;
        private readonly DirectoryInfo _rootOfGenerated;

        internal DynaType Type { get; }

        public static ProxyGenerator ForClassPath(
            IList<FileInfo> classPath,
            DirectoryInfo destinationDirectory,
            DynaType type,
            bool persist,
            ILogger logger)
            => new ProxyGenerator(classPath, destinationDirectory, type, persist, logger);

        public static ProxyGenerator ForMain(bool persist, ILogger logger)
        {
            var classPath = new List<FileInfo>
            {
                new FileInfo(Properties.Instance.GetProperty("proxy.generated.classes.main", RootOfMainClasses)!)
            };
            var type = DynaType.Main;
            var rootOfGenerated = RootOfGeneratedSources(type);

            return new ProxyGenerator(classPath, rootOfGenerated, type, persist, logger);
        }

        public static ProxyGenerator ForTest(bool persist, ILogger logger)
        {
            var classPath = new List<FileInfo>
            {
                new FileInfo(Properties.Instance.GetProperty("proxy.generated.classes.test", RootOfTestClasses)!)
            };
            var type = DynaType.Test;
            var rootOfGenerated = RootOfGeneratedSources(type);

            return new ProxyGenerator(classPath, rootOfGenerated, type, persist, logger);
        }

        public Result GenerateFor(Type actorProtocol, MethodInfo[]? methods = null)
        {
            _logger.Debug("vlingo-net/actors: Generating proxy for " + (Type == DynaType.Main ? "main" : "test") + ": " + actorProtocol.Name);
            try
            {
                var proxyClassSource = ProxyClassSource(actorProtocol, methods);
                var fullyQualifiedClassName = FullyQualifiedClassNameFor(actorProtocol, "__Proxy");
                var relativeTargetFile = ToFullPath(fullyQualifiedClassName);
                var sourceFile = _persist ?
                    PersistProxyClassSource(fullyQualifiedClassName, relativeTargetFile, proxyClassSource) :
                    new FileInfo(relativeTargetFile);

                return new Result(fullyQualifiedClassName, ClassNameFor(actorProtocol, "__Proxy"), proxyClassSource, sourceFile);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Cannot generate proxy class for: {actorProtocol.Name}", ex);
            }
        }
        
        internal static int? Seed { get; set; }

        private static DirectoryInfo RootOfGeneratedSources(DynaType type)
            => type == DynaType.Main ?
                new DirectoryInfo(Properties.Instance.GetProperty("proxy.generated.sources.main", GeneratedSources)!) :
                new DirectoryInfo(Properties.Instance.GetProperty("proxy.generated.sources.test", GeneratedTestSources)!);

        private ProxyGenerator(IList<FileInfo> rootOfClasses, DirectoryInfo rootOfGenerated, DynaType type, bool persist, ILogger logger)
        {
            _rootOfGenerated = rootOfGenerated;
            Type = type;
            _persist = persist;
            _logger = logger;
        }

        private string ClassStatement(Type protocolInterface)
            => string.Format("public class {0} : {1}{2}\n{{",
                ClassNameFor(protocolInterface, "__Proxy"),
                TypeFullNameToString(GetSimpleTypeName(protocolInterface)),
                GetGenericConstraints(protocolInterface));

        private string Constructor(Type protocolInterface)
        {
            var builder = new StringBuilder();

            var constructorName = ClassNameFor(protocolInterface, "__Proxy");
            if (protocolInterface.IsGenericType)
            {
                constructorName = constructorName.Substring(0, constructorName.IndexOf('<'));
            }

            var signature = string.Format("  public {0}(Actor actor, IMailbox mailbox)", constructorName);

            builder
                .Append(signature).Append("\n")
                .Append("  {\n")
                .Append("    this.actor = actor;\n")
                .Append("    this.mailbox = mailbox;\n")
                .Append("  }");

            return builder.ToString();
        }

        private string ImportStatements()
        {
            var namespaces = new HashSet<string>();
            namespaces.Add("System");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.Threading.Tasks");
            namespaces.Add(typeof(Actor).Namespace!);
            namespaces.Add(typeof(AtomicBoolean).Namespace!); // Vlingo.Common

            return string.Join("\n", namespaces.Select(x => $"using {x};"));
            
        }

        private string InstanceVariables()
        {
            var builder = new StringBuilder();
            builder
                .Append("  private readonly Actor actor;\n")
                .Append("  private readonly IMailbox mailbox;\n");

            return builder.ToString();
        }

        private string RepresentationStatements(IEnumerable<MethodInfo> methods)
        {
            var builder = new StringBuilder();

            var count = 0;

            foreach (var method in methods)
            {
                if (!method.IsStatic)
                {
                    var statement = string.Format("  private const string {0}Representation{1} = \"{2}({3})\";\n",
                                    method.Name,
                                    ++count,
                                    GetMethodName(method),
                                    string.Join(", ", method.GetParameters().Select(p => TypeFullNameToString(GetSimpleTypeName(p.ParameterType)))));

                    builder.Append(statement);
                }
            }

            return builder.ToString();
        }

        private string GetPropertyDefinition(PropertyInfo property)
        {
            var declaration = $"  public {TypeFullNameToString(GetSimpleTypeName(property.PropertyType))} {PrefixReservedKeywords(property.Name)}";

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

        private string GetMethodDefinition(Type protocolInterface, MethodInfo method, bool isAsync, int count)
        {
            var randomVarNumber = (Seed.HasValue ? new Random(Seed.Value) : new Random()).Next(0, int.MaxValue);
            var isACompletes = DoesImplementICompletes(method.ReturnType);
            var isTask = IsTask(method.ReturnType);
            var isAsyncAwaitCompletes = !isTask && isAsync;

            var methodParamSignature = string.Join(", ", method.GetParameters().Select(p => $"{TypeFullNameToString(GetSimpleTypeName(p.ParameterType))} {PrefixReservedKeywords(p.Name)}"));
            var methodSignature = string.Format("  public {0} {1}({2}){3}",
                TypeFullNameToString(GetSimpleTypeName(method.ReturnType)),
                GetMethodName(method),
                methodParamSignature,
                GetGenericConstraints(method));

            var ifNotStopped = "    if(!this.actor.IsStopped)\n    {";
            var consumerStatement = isTask ?
                string.Format("      var tcs = new TaskCompletionSource<{0}>();\n" +
                              "      Func<{1}, {0}> cons{4} = __ =>\n" +
                              "      {{\n" +
                              "          tcs.SetResult(__.{2}({3}));\n" +
                              "          return tcs.Task.Unwrap();\n" +
                              "      }};\n" +
                              "      Action<{1}> asyncWrapper = m =>\n" +
                              "      {{\n" +
                              "          Task Wrap() => cons{4}(m);\n" +
                              "          ExecutorDispatcherAsync.RunTask<{1}>(Wrap, mailbox, actor);\n" +
                              "      }};\n",
                    TypeFullNameToString(GetSimpleTypeName(method.ReturnType)),
                    TypeFullNameToString(GetSimpleTypeName(protocolInterface)),
                    GetMethodName(method),
                    string.Join(", ", method.GetParameters().Select(p => PrefixReservedKeywords(p.Name))),
                    randomVarNumber) : 
                        isAsyncAwaitCompletes ?
                            string.Format("      var tcs = new TaskCompletionSource<{0}>();\n" +
                                          "      Func<{1}, {0}> cons{4} = __ =>\n" +
                                          "      {{\n" +
                                          "          var returnCompletes = __.{2}({3});\n" +
                                          "          tcs.SetResult(returnCompletes);\n" +
                                          "          return returnCompletes;\n" +
                                          "      }};\n" +
                                          "      Action<{1}> asyncWrapper = m =>\n" +
                                          "      {{\n" +
                                          "          Task Wrap() => ((BasicCompletes<int>)cons{4}(m)).ToTask();\n" +
                                          "          ExecutorDispatcherAsync.RunTask<{1}>(Wrap, mailbox, actor);\n" +
                                          "      }};\n",
                                TypeFullNameToString(GetSimpleTypeName(method.ReturnType)),
                                TypeFullNameToString(GetSimpleTypeName(protocolInterface)),
                                GetMethodName(method),
                                string.Join(", ", method.GetParameters().Select(p => PrefixReservedKeywords(p.Name))),
                                randomVarNumber) :
                                string.Format("      Action<{0}> cons{3} = __ => __.{1}({2});",
                                    TypeFullNameToString(GetSimpleTypeName(protocolInterface)),
                                    GetMethodName(method),
                                    string.Join(", ", method.GetParameters().Select(p => PrefixReservedKeywords(p.Name))),
                                    randomVarNumber);
            var completesStatement = isACompletes ? string.Format("      var completes = new BasicCompletes<{0}>(this.actor.Scheduler);\n", TypeFullNameToString(GetSimpleTypeName(method.ReturnType.GetGenericArguments().First()))) : "";
            var representationName = string.Format("{0}Representation{1}", method.Name, count);
            var mailboxConsumer = isTask || isAsyncAwaitCompletes ? "asyncWrapper" : $"cons{randomVarNumber}";
            var mailboxSendStatement = string.Format(
                "      if(this.mailbox.IsPreallocated)\n" +
                "      {{\n" +
                "        this.mailbox.Send(this.actor, {4}, {0}, {1});\n" +
                "      }}\n" +
                "      else\n" +
                "      {{\n" +
                "        this.mailbox.Send(new LocalMessage<{2}>(this.actor, {4}, {3}{1}));\n" +
                "      }}",
                isACompletes ? "completes" : "null",
                representationName,
                TypeFullNameToString(GetSimpleTypeName(protocolInterface)),
                isACompletes ? "completes, " : "",
                mailboxConsumer);
            var completesReturnStatement = isACompletes ? (isAsyncAwaitCompletes ? "      return tcs.Task.Result;\n" : "      return completes;\n") : "";
            var taskReturnStatement = isTask ? "      return tcs.Task.Unwrap();\n" : "";
            var elseDead = string.Format("      this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, {0}));", representationName);
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
                .Append(taskReturnStatement)
                .Append("    }\n")
                .Append("    else\n")
                .Append("    {\n")
                .Append(elseDead).Append("\n")
                .Append("    }\n")
                .Append(returnStatement)
                .Append("  }\n");

            return builder.ToString();
        }

        private string MethodDefinitions(Type protocolInterface, IEnumerable<(MethodInfo m, bool)> methods)
        {
            var builder = new StringBuilder();
            int count = 0;
            foreach(var (method, isAsync) in methods)
            {
                builder.Append(GetMethodDefinition(protocolInterface, method, isAsync, ++count));
            }

            return builder.ToString();
        }

        private string NamespaceStatement(Type protocolInterface, bool hasNamespace) => hasNamespace ? $"namespace {protocolInterface.Namespace}\n{{" : string.Empty;

        private FileInfo PersistProxyClassSource(string fullyQualifiedClassName, string relativePathToClass, string proxyClassSource)
        {
            var pathToGeneratedSource = ToNamespacePath(fullyQualifiedClassName);
            var dir = new DirectoryInfo(_rootOfGenerated + pathToGeneratedSource);

            if (!dir.Exists)
            {
                dir.Create();
            }

            var pathToSource = _rootOfGenerated + relativePathToClass + ".cs";

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

        private string ProxyClassSource(Type protocolInterface, MethodInfo[]? actorMethods)
        {
            var hasNamespace = !string.IsNullOrWhiteSpace(protocolInterface.Namespace);
            var methods = actorMethods != null ? 
                GetAbstractMethodsFor(protocolInterface)
                    .Select(m => (m, actorMethods.Contains(m, new MethodInfoComparer()))).ToList() : 
                GetAbstractMethodsFor(protocolInterface).Select(m => (m, false)).ToList();
            var properties = GetPropertiesFor(protocolInterface).ToList();
            var builder = new StringBuilder();
            builder
                .Append(ImportStatements()).Append("\n")
                .Append(NamespaceStatement(protocolInterface, hasNamespace)).Append("\n")
                .Append(ClassStatement(protocolInterface)).Append("\n")
                .Append(RepresentationStatements(methods.Select(m => m.Item1))).Append("\n")
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

        private string? DefaultReturnValueString(Type type)
        {
            if(type == typeof(void))
            {
                return string.Empty;
            }

            if (type.IsGenericParameter)
            {
                return $"default({type.Name})";
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
                return $"{type.Name}.{Activator.CreateInstance(type)}";
            }

            return Activator.CreateInstance(type)?.ToString();
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

        private static readonly string[] ReservedKeywords = {
            "object",
            "event",
            "struct",
            "class",
            "const",
            "delegate",
            "interface",
            "protected",
            "static",
            "sealed",
            "string",
            "double",
            "checked"
        };

        private string GetSimpleTypeName(Type type)
        {
            if (SimpleTypeNames.ContainsKey(type))
            {
                return SimpleTypeNames[type];
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return $"{GetSimpleTypeName(Nullable.GetUnderlyingType(type)!)}?";
            }

            if (type.IsGenericType)
            {
                var typeName = type.FullName ?? type.Name;
                var name = typeName.Substring(0, typeName.IndexOf('`'));
                var genericTypeDeclaration = string.Join(", ", type.GetGenericArguments().Select(GetSimpleTypeName));
                return $"{name}<{genericTypeDeclaration}>";
            }

            return type.FullName ?? type.Name;
        }

        private string TypeFullNameToString(string typeFullName) => typeFullName.Replace("+", ".");

        private string? PrefixReservedKeywords(string? name)
        {
            if (ReservedKeywords.Contains(name))
            {
                return $"@{name}";
            }

            return name;
        }

        private string GetMethodName(MethodInfo info)
        {
            if (info.IsGenericMethod)
            {
                var genericArguments = info.GetGenericArguments();
                return $"{info.Name}<{string.Join(", ", genericArguments.Select(a => a.Name))}>";
            }

            return info.Name;
        }
        
        private string GetGenericConstraints(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                var constraintPairs = type.GetGenericTypeDefinition()
                    .GetGenericArguments()
                    .Select(i =>
                    {
                        var genericConstraints = i.GetGenericParameterConstraints().Select(c => c.FullName).ToList();
                        if (!genericConstraints.Any())
                        {
                            var gpa = i.GenericParameterAttributes;
                            var constraintMask = gpa & GenericParameterAttributes.SpecialConstraintMask;
                            if (constraintMask != GenericParameterAttributes.None)
                            {
                                if ((constraintMask & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                                    genericConstraints.Add("class");
                                if ((constraintMask & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                                    genericConstraints.Add("struct");
                                if ((constraintMask & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                                    genericConstraints.Add("new()");
                            }
                        }
                        return (i.Name, string.Join(", ", genericConstraints));
                    })
                    .Where(p => !string.IsNullOrEmpty(p.Item2));

                var constraints = constraintPairs.Select(p => $" where {p.Item1} : {p.Item2}");
                return string.Join(" ", constraints);
            }

            return string.Empty;
        }

        private string GetGenericConstraints(MethodInfo info)
        {
            if (info.IsGenericMethod)
            {
                var constraintPairs = info.GetGenericMethodDefinition()
                    .GetGenericArguments()
                    .Select(i => (i.Name, string.Join(", ", i.GetGenericParameterConstraints().Select(c => c.FullName))));

                var validConstraints = constraintPairs.Where(p => !string.IsNullOrEmpty(p.Item2)).ToList();
                
                if (validConstraints.Count == 0)
                {
                    return string.Empty;
                }

                var constraints = validConstraints.Select(p => $" where {p.Item1} : {p.Item2}");
                return string.Join(" ", constraints);
            }

            return string.Empty;
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

            if (interfaces.Length == 0)
            {
                return false;
            }

            return interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == completesUnboundedType);
        }
        
        private static bool IsTask(Type type)
        {
            return type == typeof(Task) || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
        }

        private class MethodInfoComparer : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo? x, MethodInfo? y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Name == y.Name)
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(MethodInfo obj) => 31 * obj.Name.GetHashCode();
        }
    }
}
