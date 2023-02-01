// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Logging.NoOp;

public class NoOpLogger : ILogger
{
    public bool IsEnabled => false;

    public string Name => "no-op";

    public void Close()
    {
    }

    public void Debug(string message)
    {
    }

    public void Debug(string message, params object[] args)
    {
    }

    public void Debug(string message, Exception exception)
    {
    }

    public void Error(string message)
    {
    }

    public void Error(string message, params object[] args)
    {
    }

    public void Error(string message, Exception exception)
    {
    }

    public void Trace(LogEvent logEvent)
    {
    }

    public void Debug(LogEvent logEvent)
    {
    }

    public void Info(LogEvent logEvent)
    {
    }

    public void Warn(LogEvent logEvent)
    {
    }

    public void Error(LogEvent logEvent)
    {
    }

    public void Info(string message)
    {
    }

    public void Info(string message, params object[] args)
    {
    }

    public void Info(string message, Exception exception)
    {
    }

    public void Trace(string message)
    {
    }

    public void Trace(string message, params object[] args)
    {
    }

    public void Trace(string message, Exception exception)
    {
    }

    public void Warn(string message)
    {
    }

    public void Warn(string message, params object[] args)
    {
    }

    public void Warn(string message, Exception exception)
    {
    }
}