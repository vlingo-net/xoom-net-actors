// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Actors.Plugin.Logging;

namespace Vlingo.Actors
{
    public class ActorLoggerAdapter : ILogger
    {
        private readonly ILogger _logger;
        private readonly IAddress? _sourceActorAddress;
        private readonly Type _sourceActorType;
        
        public static ActorLoggerAdapter From(ILogger logger, Type sourceActorType)
            => new ActorLoggerAdapter(logger, null, sourceActorType);
        
        public static ActorLoggerAdapter From(ILogger logger, IAddress sourceActorAddress, Type sourceActorType)
            => new ActorLoggerAdapter(logger, sourceActorAddress, sourceActorType);
        
        private ActorLoggerAdapter(ILogger logger, IAddress? sourceActorAddress, Type sourceActorType)
        {
            _logger = logger;
            _sourceActorAddress = sourceActorAddress;
            _sourceActorType = sourceActorType;
        }

        public bool IsEnabled => _logger.IsEnabled;
        public string Name => _logger.Name;
        public void Close() => _logger.Close();

        public void Trace(string message)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).Build();
            Trace(logEvent);
        }

        public void Trace(string message, params object[] args)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithArgs(args).Build();
            Trace(logEvent);
        }

        public void Trace(string message, Exception exception)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithException(exception).Build();
            Trace(logEvent);
        }

        public void Debug(string message)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).Build();
            Debug(logEvent);
        }

        public void Debug(string message, params object[] args)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithArgs(args).Build();
            Debug(logEvent);
        }

        public void Debug(string message, Exception exception)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithException(exception).Build();
            Debug(logEvent);
        }

        public void Info(string message)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).Build();
            Info(logEvent);
        }

        public void Info(string message, params object[] args)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithArgs(args).Build();
            Info(logEvent);
        }

        public void Info(string message, Exception exception)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithException(exception).Build();
            Info(logEvent);
        }

        public void Warn(string message)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).Build();
            Warn(logEvent);
        }

        public void Warn(string message, params object[] args)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithArgs(args).Build();
            Warn(logEvent);
        }

        public void Warn(string message, Exception exception)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithException(exception).Build();
            Warn(logEvent);
        }

        public void Error(string message)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).Build();
            Error(logEvent);
        }

        public void Error(string message, params object[] args)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithArgs(args).Build();
            Error(logEvent);
        }

        public void Error(string message, Exception exception)
        {
            var logEvent = new LogEvent.Builder(_sourceActorType, message).WithSourceActorAddress(_sourceActorAddress).WithException(exception).Build();
            Error(logEvent);
        }

        public void Trace(LogEvent logEvent) => _logger.Trace(logEvent);

        public void Debug(LogEvent logEvent) => _logger.Debug(logEvent);

        public void Info(LogEvent logEvent) => _logger.Info(logEvent);

        public void Warn(LogEvent logEvent) => _logger.Warn(logEvent);

        public void Error(LogEvent logEvent) => _logger.Error(logEvent);
    }
}