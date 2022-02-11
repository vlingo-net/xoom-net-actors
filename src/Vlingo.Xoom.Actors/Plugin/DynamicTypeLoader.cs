// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Reflection;

namespace Vlingo.Xoom.Actors.Plugin;

public class DynamicTypeLoader : IPluginTypeLoader
{
    private readonly string[] _assemblyPaths;

    public DynamicTypeLoader(string[] assemblyPaths) => _assemblyPaths = assemblyPaths;

    public Type LoadType(string name)
    {
        foreach (var assemblyPath in _assemblyPaths)
        {
            try
            {
                var assembly = Assembly.LoadFile(assemblyPath);
                var type = assembly.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception)
            {
                // nothing to do
            }
        }

        throw new FileNotFoundException($"Could not load assembly for given paths '{string.Join(",", _assemblyPaths)}'");
    }

    public Stream? LoadResource(string resourceName)
    {
        foreach (var assemblyPath in _assemblyPaths)
        {
            try
            {
                var assembly = Assembly.LoadFile(assemblyPath);
                var path = $"{resourceName.Replace("/", ".").Replace(" ", "_")}";
                var stream = assembly.GetManifestResourceStream(path);
                return stream;
            }
            catch (Exception)
            {
                // nothing to do
            }
        }
        
        throw new FileNotFoundException($"Could not load assembly for given paths '{string.Join(",", _assemblyPaths)}'");
    }
}