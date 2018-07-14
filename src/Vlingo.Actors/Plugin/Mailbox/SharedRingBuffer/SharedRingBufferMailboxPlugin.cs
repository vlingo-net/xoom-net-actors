using System;

namespace Vlingo.Actors.Plugin.Mailbox.SharedRingBuffer
{
    public class SharedRingBufferMailboxPlugin : IPlugin, IMailboxProvider
    {
        private RingBufferDispatcherPool dispatcherPool;

        public string Name { get; private set; }

        public int Pass => 1;

        public void Close()
        {
            dispatcherPool.Close();
        }

        public IMailbox ProvideMailboxFor(int hashCode) => dispatcherPool.AssignFor(hashCode).Mailbox;

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher) => dispatcherPool.AssignFor(hashCode).Mailbox;

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;
            CreateDispatcherPool(properties);
            RegisterWith(registrar, properties);
        }

        private void CreateDispatcherPool(PluginProperties properties)
        {
            var numberOfDispatchersFactor = properties.GetFloat("numberOfDispatchersFactor", 1.5f);
            var size = properties.GetInteger("size", 1048576);
            var fixedBackoff = properties.GetInteger("fixedBackoff", 2);
            var dispatcherThrottlingCount = properties.GetInteger("dispatcherThrottlingCount", 1);

            dispatcherPool = new RingBufferDispatcherPool(
                System.Environment.ProcessorCount,
                numberOfDispatchersFactor,
                size,
                fixedBackoff,
                dispatcherThrottlingCount);
        }

        private void RegisterWith(IRegistrar registrar, PluginProperties properties)
        {
            var defaultMailbox = properties.GetBoolean("defaultMailbox", true);

            registrar.Register(Name, defaultMailbox, this);
        }
    }
}
