using System;
using Vlingo.Common.Compiler;

namespace Vlingo.Actors.Plugin.Supervision
{
    internal class ConfiguredSupervisor
    {
        private static readonly Lazy<DynaClassLoader> ClassLoaderSingleton = new Lazy<DynaClassLoader>(
            () => new DynaClassLoader(), true);

        private static DynaClassLoader ClassLoader => ClassLoaderSingleton.Value;

        internal static Type ProtocolFrom(string supervisedProtocol)
        {
            try
            {
                return ClassLoader.LoadClass(supervisedProtocol);
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Cannot load class for: {supervisedProtocol}");
            }
        }

        internal static Type SupervisorFrom(string supervisorClassname)
        {
            try
            {
                var type = ClassLoader.LoadClass(supervisorClassname);
                if (typeof(Actor).IsAssignableFrom(type))
                {
                    return type;
                }
                throw new InvalidOperationException();
            }
            catch (Exception)
            {
                throw new InvalidOperationException($"Cannot load class for: {supervisorClassname}");
            }
        }

        internal string StageName { get; }

        internal string SupervisorName { get; }

        internal Type SupervisorClass { get; }

        internal Type SupervisedProtocol { get; }

        public override int GetHashCode()
            => 31 * StageName.GetHashCode() + SupervisorName.GetHashCode();

        public override bool Equals(object other)
        {
            if (other == null || other.GetType() != this.GetType())
            {
                return false;
            }

            var otherSupervisor = (ConfiguredSupervisor)other;

            return StageName.Equals(otherSupervisor.StageName) &&
                   SupervisorName.Equals(otherSupervisor.SupervisorName) &&
                   ((SupervisedProtocol == null && otherSupervisor.SupervisedProtocol == null) ||
                   (SupervisedProtocol != null && otherSupervisor.SupervisedProtocol != null &&
                    SupervisedProtocol.Equals(otherSupervisor.SupervisedProtocol))) &&
                   SupervisorClass.Equals(otherSupervisor.SupervisorClass);
        }

        internal ConfiguredSupervisor(string stageName, string supervisorName, Type supervisedProtocol, Type supervisorClass)
        {
            StageName = stageName;
            SupervisorName = supervisorName;
            SupervisedProtocol = supervisedProtocol;
            SupervisorClass = supervisorClass;
        }

        internal ConfiguredSupervisor(string stageName, string supervisorName, string supervisedProtocol, string supervisorClassname)
        {
            StageName = stageName;
            SupervisorName = supervisorName;
            SupervisedProtocol = ProtocolFrom(supervisedProtocol);
            SupervisorClass = SupervisorFrom(supervisorClassname);
        }

        internal ConfiguredSupervisor(string stageName, string supervisorName, Type supervisorClass)
        {
            StageName = stageName;
            SupervisorName = supervisorName;
            SupervisedProtocol = null;
            SupervisorClass = supervisorClass;
        }

        internal ConfiguredSupervisor(string stageName, string supervisorName, string supervisorClassname)
        {
            StageName = stageName;
            SupervisorName = supervisorName;
            SupervisedProtocol = null;
            SupervisorClass = SupervisorFrom(supervisorClassname);
        }
    }
}
