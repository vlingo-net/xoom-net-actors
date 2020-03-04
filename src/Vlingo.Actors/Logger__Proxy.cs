// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public class Logger__Proxy : ILogger
    {
        private const string NameRepresentation1 = "Name";
        private const string CloseRepresentation4 = "Close()";
        private const string IsEnabledRepresentation5 = "IsEnabled";
        private const string TraceRepresentation1 = "Trace(string)";
        private const string TraceRepresentation2 = "Trace(string, params object[] args)";
        private const string TraceRepresentation3 = "Trace(string, Exception)";
        private const string DebugRepresentation1 = "Debug(string)";
        private const string DebugRepresentation2 = "Debug(string, params object[] args)";
        private const string DebugRepresentation3 = "Debug(string, Exception)";
        private const string InfoRepresentation1 = "Info(string)";
        private const string InfoRepresentation2 = "Info(string, params object[] args)";
        private const string InfoRepresentation3 = "Info(string, Exception)";
        private const string WarnRepresentation1 = "Warn(string)";
        private const string WarnRepresentation2 = "Warn(string, params object[] args)";
        private const string WarnRepresentation3 = "Warn(string, Exception)";
        private const string ErrorRepresentation1 = "Error(string)";
        private const string ErrorRepresentation2 = "Error(string, params object[] args)";
        private const string ErrorRepresentation3 = "Error(string, Exception)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Logger__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public bool IsEnabled => false;

        public string Name => null!;

        public void Close()
            => Send(x => x.Close(), CloseRepresentation4);

        public void Debug(string message)
            => Send(x => x.Debug(message), DebugRepresentation1);

        public void Debug(string message, params object[] args)
            => Send(x => x.Debug(message, args), DebugRepresentation2);

        public void Debug(string message, Exception throwable)
            => Send(x => x.Debug(message, throwable), DebugRepresentation3);

        public void Error(string message)
            => Send(x => x.Error(message), ErrorRepresentation1);

        public void Error(string message, params object[] args)
            => Send(x => x.Error(message, args), ErrorRepresentation2);

        public void Error(string message, Exception throwable)
            => Send(x => x.Error(message, throwable), ErrorRepresentation3);

        public void Info(string message)
            => Send(x => x.Info(message), InfoRepresentation1);

        public void Info(string message, params object[] args)
            => Send(x => x.Info(message, args), InfoRepresentation2);

        public void Info(string message, Exception throwable)
            => Send(x => x.Info(message, throwable), InfoRepresentation3);

        public void Trace(string message)
            => Send(x => x.Trace(message), TraceRepresentation1);

        public void Trace(string message, params object[] args)
            => Send(x => x.Trace(message, args), TraceRepresentation2);

        public void Trace(string message, Exception throwable)
            => Send(x => x.Trace(message, throwable), TraceRepresentation3);

        public void Warn(string message)
            => Send(x => x.Warn(message), WarnRepresentation1);

        public void Warn(string message, params object[] args)
            => Send(x => x.Warn(message, args), WarnRepresentation2);

        public void Warn(string message, Exception throwable)
            => Send(x => x.Warn(message, throwable), WarnRepresentation3);

        private void Send(Action<ILogger> consumer, string representation)
        {
            if (!actor.IsStopped)
            {
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, null, representation);
                }
                else
                {
                    mailbox.Send(new LocalMessage<ILogger>(actor, consumer, representation));
                }
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, representation));
            }
        }
    }
}
