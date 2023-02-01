// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors.Plugin.Logging.Console;

public class ConsoleLoggerActor : Actor, ILogger
{
    private readonly ConsoleLogger _logger;

    public ConsoleLoggerActor(ConsoleLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Close() => _logger.Close();

    public bool IsEnabled => _logger.IsEnabled;

    public string Name => _logger.Name;

    public override void Stop()
    {
        Close();
        base.Stop();
    }

    public void Trace(string message) => _logger.Trace(message);

    public void Trace(string message, params object[] args) => _logger.Trace(message, args);

    public void Trace(string message, Exception exception) => _logger.Trace(message, exception);

    public void Debug(string message) => _logger.Debug(message);

    public void Debug(string message, params object[] args) => _logger.Debug(message, args);

    public void Debug(string message, Exception exception) => _logger.Debug(message, exception);

    public void Info(string message) => _logger.Info(message);

    public void Info(string message, params object[] args) => _logger.Info(message, args);

    public void Info(string message, Exception exception) => _logger.Info(message, exception);

    public void Warn(string message) => _logger.Warn(message);

    public void Warn(string message, params object[] args) => _logger.Warn(message, args);

    public void Warn(string message, Exception exception) => _logger.Warn(message, exception);

    public void Error(string message) => _logger.Error(message);

    public void Error(string message, params object[] args) => _logger.Error(message, args);

    public void Error(string message, Exception exception) => _logger.Error(message, exception);

    public void Trace(LogEvent logEvent) => _logger.Trace(logEvent);

    public void Debug(LogEvent logEvent) => _logger.Debug(logEvent);

    public void Info(LogEvent logEvent) => _logger.Info(logEvent);

    public void Warn(LogEvent logEvent) => _logger.Warn(logEvent);

    public void Error(LogEvent logEvent) => _logger.Error(logEvent);
}