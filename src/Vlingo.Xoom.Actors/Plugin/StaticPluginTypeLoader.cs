// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Reflection;

namespace Vlingo.Xoom.Actors.Plugin;

public class StaticPluginTypeLoader : IPluginTypeLoader
{
    public Type? LoadType(string name) => Type.GetType(name);

    public Stream? LoadResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var path = $"{resourceName.Replace("/", ".").Replace(" ", "_")}";
        var stream = assembly.GetManifestResourceStream(path);
        return stream;
    }
}