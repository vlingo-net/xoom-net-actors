namespace Vlingo.Actors.Plugin.Supervision
{
    public class DefaultSupervisorOverridePlugin : IPlugin
    {
        public string Name { get; private set; }

        public int Pass => 2;

        public void Close()
        {
        }

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;

            foreach(var value in DefinitionValues.AllDefinitionValues(properties))
            {
                registrar.RegisterDefaultSupervisor(value.StageName, value.Name, value.Supervisor);
            }
        }
    }
}
