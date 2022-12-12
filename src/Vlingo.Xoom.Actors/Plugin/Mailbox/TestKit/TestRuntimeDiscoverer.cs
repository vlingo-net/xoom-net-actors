// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Mailbox.TestKit;

/// <summary>
/// Discover whether the runtime is currently under test.
/// </summary>
public class TestRuntimeDiscoverer
{
    private static bool _runningFromXunit;
    
    public static bool IsTestDetectionDeactivated { get; set; }
    
    public static bool IsUnderTest()
    {
        if (IsTestDetectionDeactivated)
        {
            return false;
        }
        
        foreach (var assem in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Can't do something like this as it will load the xunit assembly
            // if (assem == typeof(Xunit.Assert))

            if (assem.FullName!.ToLowerInvariant().StartsWith("xunit"))
            {
                _runningFromXunit = true;
                break;
            }
        }

        return _runningFromXunit;
    }
    
    public static bool IsUnderTestWith(string className, string methodName) => 
        throw new NotSupportedException("Currently not implemented");
}