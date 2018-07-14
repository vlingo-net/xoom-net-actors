namespace Vlingo.Actors.Plugin.Supervision
{
    public class CommonSupervisorsPlugin : IPlugin
    {
        public string Name { get; private set; }

        public int Pass => 2;

        public void Close()
        {
        }

        public void Start(IRegistrar registrar, string name, PluginProperties properties)
        {
            Name = name;

            foreach (var value in DefinitionValues.AllDefinitionValues(properties))
            {
                registrar.RegisterCommonSupervisor(value.StageName, value.Name, value.Protocol, value.Supervisor);
            }
        }
    }
}
