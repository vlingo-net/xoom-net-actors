using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors.Plugin
{
    public interface IPlugin
    {
        void Close();
        string Name { get; }
        int Pass { get; }
        void Start(IRegistrar registrar, string name, PluginProperties properties);
    }
}
