// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using System.Threading;

namespace Vlingo.Actors.Logging
{
    public class LogEvent
    {
        public Type Source { get; }

        public string Message { get; }
        public object[]? Args { get; }
        public Exception? Exception { get; }

        //MDC fields
        public int SourceThread { get; }
        public DateTimeOffset EventOccuredOn { get; }
        public IAddress? SourceActorAddress { get; }

        public LogEvent(Type source, int sourceThread, DateTimeOffset eventOccuredOn, string message, object[]? args, Exception? exception, IAddress? sourceActorAddress)
        {
            Source = source;
            Message = message;
            Args = args;
            Exception = exception;
            SourceThread = sourceThread;
            EventOccuredOn = eventOccuredOn;
            SourceActorAddress = sourceActorAddress;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Source: {Source}\n")
                .Append($"Message: {Message}\n")
                .Append($"SourceThread: {SourceThread}\n")
                .Append($"OccuredOn: {EventOccuredOn:MM/dd/yyyy hh:mm:ss.fff}\n");

            if (SourceActorAddress != null)
            {
                sb.Append($"Address: {SourceActorAddress}\n");
            }

            if (Args != null && Args.Length > 0)
            {
                sb.Append($"Args: ");
                foreach (var arg in Args)
                {
                    sb.Append($"{arg}, ");
                }

                sb.Append("\n");
            }

            if (Exception != null)
            {
                sb.Append($"Exception: {Exception.Message}\n")
                    .Append($"StackTrace: {Exception.StackTrace}\n");
                
                if (Exception.InnerException != null)
                {
                    sb.Append($"InnerException: {Exception.InnerException.Message}\n")
                        .Append($"StackTrace: {Exception.InnerException.StackTrace}\n");
                }
            }

            return sb.ToString();
        }

        public class Builder
        {
            private readonly Type _source;
            private readonly string _message;
            private readonly int _sourceThread;
            private readonly DateTimeOffset _eventOccuredOn;
            private object[]? _args;
            private Exception? _exception;
            private IAddress? _sourceActorAddress;

            public Builder(Type source, string message, int sourceThread, DateTimeOffset eventOccuredOn)
            {
                _source = source;
                _message = message;
                _sourceThread = sourceThread;
                _eventOccuredOn = eventOccuredOn;
            }

            public Builder(Type source, string message)
            {
                _source = source;
                _message = message;
                _sourceThread = Thread.CurrentThread.ManagedThreadId;
                _eventOccuredOn = DateTimeOffset.Now;
            }

            public Builder WithArgs(params object[] args)
            {
                _args = args;
                return this;
            }

            public Builder WithException(Exception exception)
            {
                _exception = exception;
                return this;
            }

            public Builder WithSourceActorAddress(IAddress sourceActorAddress)
            {
                _sourceActorAddress = sourceActorAddress;
                return this;
            }

            public LogEvent Build() => new LogEvent(_source, _sourceThread, _eventOccuredOn, _message, _args, _exception, _sourceActorAddress);
        }
    }
}