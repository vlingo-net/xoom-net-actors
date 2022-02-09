// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors.Plugin
{
    public abstract class AbstractPlugin : IPlugin
    {
        public abstract string Name { get; }
        public abstract int Pass { get; }
        public abstract IPluginConfiguration Configuration { get; }

        public abstract void Close();
        public abstract void Start(IRegistrar registrar);
        public abstract IPlugin With(IPluginConfiguration? overrideConfiguration);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            return string.Equals(Name, ((IPlugin)obj).Name);
        }

        public override int GetHashCode() =>
            $"{GetType().FullName}::{Name}".GetHashCode();
    }
}
