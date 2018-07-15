// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Plugin.Logging.NoOp
{
    public class NoOpLogger : ILogger
    {
        public bool IsEnabled => false;

        public string Name => "no-op";

        public void Close()
        {
        }

        public void Log(string message)
        {
        }

        public void Log(string message, Exception ex)
        {
        }
    }
}
