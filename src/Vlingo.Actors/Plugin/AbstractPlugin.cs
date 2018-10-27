// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors.Plugin
{
    public abstract class AbstractPlugin : IPlugin
    {
        public abstract string Name { get; }
        public abstract int Pass { get; }
        public abstract IPluginConfiguration Configuration { get; }

        public abstract void Close();
        public abstract void Start(IRegistrar registrar);

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return string.Equals(Name, ((IPlugin)obj).Name);
        }

        public override int GetHashCode()
        {
            return $"{GetType().FullName}::{Name}".GetHashCode();
        }
    }
}
