namespace Vlingo.Actors.Plugin.Mailbox.TestKit
{
    public class TestMailboxPlugin : IPlugin, IMailboxProvider
    {
        public TestMailboxPlugin(IRegistrar registrar)
        {
            this.Start(registrar, TestMailbox.Name, null);
        }

        public string Name { get; private set; }

        public int Pass => 1;

        public void Close()
        {
        }

        public IMailbox ProvideMailboxFor(int hashCode) => new TestMailbox();

        public IMailbox ProvideMailboxFor(int hashCode, IDispatcher dispatcher) => new TestMailbox();

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;
            registrar.Register(name, false, this);
        }
    }
}
