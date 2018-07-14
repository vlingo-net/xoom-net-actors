namespace Vlingo.Actors.Plugin.Completes
{
    public class PooledCompletesPlugin : IPlugin
    {
        private ICompletesEventuallyProvider completesEventuallyProvider;

        public string Name { get; private set; }

        public int Pass => 2;

        public void Close()
        {
            completesEventuallyProvider.Close();
        }

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;

            var poolSize = properties.GetInteger("pool", 10);
            completesEventuallyProvider = new CompletesEventuallyPool(poolSize);
            registrar.Register(name, completesEventuallyProvider);
        }
    }
}
