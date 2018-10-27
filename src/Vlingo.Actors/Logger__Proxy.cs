// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
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
        private const string LogRepresentation1 = "Log(string)";
        private const string LogRepresentation2 = "Log(string, Exception)";
        private const string CloseRepresentation3 = "Close()";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public Logger__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }
        public bool IsEnabled => false;
        public string Name => null;

        public void Log(string message)
        {
            if (!actor.IsStopped)
            {
                Action<ILogger> consumer = actor => actor.Log(message);
                mailbox.Send(new LocalMessage<ILogger>(actor, consumer, LogRepresentation1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, LogRepresentation1));
            }
        }
        public void Log(string message, Exception ex)
        {
            if (!actor.IsStopped)
            {
                Action<ILogger> consumer = actor => actor.Log(message, ex);
                mailbox.Send(new LocalMessage<ILogger>(actor, consumer, LogRepresentation2));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, LogRepresentation2));
            }
        }
        public void Close()
        {
            if (!actor.IsStopped)
            {
                Action<ILogger> consumer = actor => actor.Close();
                mailbox.Send(new LocalMessage<ILogger>(actor, consumer, CloseRepresentation3));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, CloseRepresentation3));
            }
        }
    }
}
