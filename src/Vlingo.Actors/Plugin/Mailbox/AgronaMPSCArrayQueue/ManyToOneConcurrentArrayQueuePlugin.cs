using System;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueuePlugin : IPlugin, IMailboxProvider
    {
        private ManyToOneConcurrentArrayQueueDispatcherPool dispatcherPool;

        public ManyToOneConcurrentArrayQueuePlugin()
        {
        }

        public void Close()
        {
            dispatcherPool.Close();
        }

        public string Name { get; private set; }

        public int Pass => 1;

        public bool IsClosed => throw new System.NotImplementedException();

        public bool IsDelivering => throw new System.NotImplementedException();

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;
            CreateDispatcherPool(properties);
            RegisterWith(registrar, properties);
        }

        public IMailbox ProvideMailboxFor(int hashCode) => dispatcherPool.AssignFor(hashCode).Mailbox;

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher) => dispatcherPool.AssignFor(hashCode).Mailbox;

        private void CreateDispatcherPool(PluginProperties properties)
        {
            var numberOfDispatchersFactor = properties.GetFloat("numberOfDispatchersFactor", 1.5f);
            var size = properties.GetInteger("size", 1048576);
            var fixedBackoff = properties.GetInteger("fixedBackoff", 2);
            var dispatcherThrottlingCount = properties.GetInteger("dispatcherThrottlingCount", 1);
            var totalSendRetries = properties.GetInteger("sendRetires", 10);

            dispatcherPool = new ManyToOneConcurrentArrayQueueDispatcherPool(
                System.Environment.ProcessorCount,
                numberOfDispatchersFactor,
                size,
                fixedBackoff,
                dispatcherThrottlingCount,
                totalSendRetries);
        }

        private void RegisterWith(IRegistrar registrar, PluginProperties properties)
        {
            var defaultMailbox = properties.GetBoolean("defaultMailbox", true);

            registrar.Register(Name, defaultMailbox, this);
        }
    }
}
