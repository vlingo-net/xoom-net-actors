// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Vlingo.Xoom.Actors.Plugin;

/// <summary>
/// This class scans and loads dynamically <see cref="IPlugin"/>s at runtime.
/// </summary>
public class PluginScanner
{
    private static readonly string PropertiesExtension = ".json";
    private static string _dllExtension = ".dll";
    //private static string _xoomSystemPluginsFolder = "/opt/xoom/plugins/";
    private static readonly string XoomUserPluginsFolderKey = "actors.pluginsFolder";

    private readonly Configuration _configuration;
    private readonly IRegistrar _registrar;
    private readonly ILogger _logger;

    private Timer? _executorService;
    private readonly ISet<string> _resourceNames = new HashSet<string>();

    public PluginScanner(Configuration configuration, IRegistrar registrar, ILogger logger)
    {
        _configuration = configuration;
        _registrar = registrar;
        _logger = logger;
    }
    
    public void StartScan()
    {
        if (_executorService != null)
        {
            throw new InvalidOperationException("Scanning of plugins has already been started!");
        }

        _executorService = new Timer(Scan);
        _executorService.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20));
    }

    public void StopScan()
    {
        if (_executorService == null)
        {
            throw new InvalidOperationException("Scanning of plugins has already been stopped!");
        }
        
        try
        {
            _executorService.Dispose();
        }
        catch (Exception e)
        {
            _logger.Warn($"Stopping plugin scanner failed because of {e.Message}", e);
        }
        finally
        {
            _executorService = null;
        }
    }
    
    private void Scan(object? state)
    {
        var systemPluginsPath = XoomUserPluginsFolderKey;
        var userPluginsFolder = _configuration.GetProperty(XoomUserPluginsFolderKey);
        var pluginsPaths = userPluginsFolder == null
            ? new[] {systemPluginsPath}
            : new[] {systemPluginsPath, userPluginsFolder};

        foreach (var pluginsPath in pluginsPaths)
        {
            if (File.Exists(pluginsPath) && (File.GetAttributes(pluginsPath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                try
                {
                    System.IO.Directory
                        .EnumerateFiles(pluginsPath)
                        .Select(filePath => new FileInfo(filePath))
                        .Where(f => (f.Attributes & FileAttributes.Normal) == FileAttributes.Normal && f.Extension.EndsWith(_dllExtension))
                        .ToList()
                        .ForEach(f => LoadAndStartPlugins(f.FullName));
                }
                catch (Exception e)
                {
                    _logger.Warn($"Plugins scanner of path {pluginsPath} failed because of {e.Message}", e);
                }
            }
        }
    }

    private void LoadAndStartPlugins(string resourcePath)
    {
        try
        {
            var fileName = new FileInfo(resourcePath).Name;
            var pluginTypeLoader = new DynamicTypeLoader(new []{resourcePath});
            var propertiesFileName = resourcePath.Substring(0, fileName.Length - _dllExtension.Length) + PropertiesExtension;
            var pluginProperties = new Properties();
            var propertiesStream = pluginTypeLoader.LoadResource(propertiesFileName);

            pluginProperties.Load(propertiesStream);
            _configuration.LoadAndStartDynamicPlugins(_registrar, pluginTypeLoader, pluginProperties);
            _resourceNames.Add(resourcePath);
        }
        catch (Exception e)
        {
            _logger.Warn($"Failed to load and start plugins from {resourcePath} because of {e.Message}", e);
        }
    }
}