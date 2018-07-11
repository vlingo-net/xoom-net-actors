using System;

namespace Vlingo.Actors.Plugin.Mailbox.ConcurrentQueue
{
    public class ConcurrentQueueMailboxPlugin : IPlugin, IMailboxProvider
    {
        private IDispatcher executorDispatcher;

        public ConcurrentQueueMailboxPlugin()
        {
        }

        public string Name { get; private set; }

        public int Pass => 1;

        public void Close()
        {
            executorDispatcher.Close();
        }

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;
            ConcurrentQueueMailboxSettings.With(properties.GetInteger("dispatcherThrottlingCount", 1));
            CreateExecutorDispatcher(properties);
            RegisterWith(registrar, properties);
        }

        public IMailbox ProvideMailboxFor(int hashCode) => new ConcurrentQueueMailbox(executorDispatcher);

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException("Dispatcher must not be null.");
            }

            return new ConcurrentQueueMailbox(dispatcher);
        }

        private void CreateExecutorDispatcher(PluginProperties properties)
        {
            var numberOfDispatchersFactor = properties.GetFloat("numberOfDispatchersFactor", 1.5f);

            executorDispatcher =
                new ExecutorDispatcher(
                    System.Environment.ProcessorCount,
                    numberOfDispatchersFactor);
        }

        private void RegisterWith(IRegistrar registrar, PluginProperties properties)
        {
            var defaultMailbox = properties.GetBoolean("defaultMailbox", true);
            registrar.Register(Name, defaultMailbox, this);
        }
    }
}
