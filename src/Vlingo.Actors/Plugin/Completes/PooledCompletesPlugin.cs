﻿// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

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