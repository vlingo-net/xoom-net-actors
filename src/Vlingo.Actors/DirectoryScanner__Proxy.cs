// Copyright (c) 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class DirectoryScanner__Proxy : IDirectoryScanner
    {
        private const string ActorOfRepresentation1 = "ActorOf<T>(Vlingo.Actors.Address)";
        private const string ActorOfRepresentation2 = "MaybeActorOf<T>(Vlingo.Actors.Address)";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public DirectoryScanner__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public ICompletes<T> ActorOf<T>(IAddress address)
        {
            if (!actor.IsStopped)
            {
                Action<IDirectoryScanner> consumer = x => x.ActorOf<T>(address);
                var completes = new BasicCompletes<T>(actor.Scheduler);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, completes, ActorOfRepresentation1);
                }
                else
                {
                    mailbox.Send(new LocalMessage<IDirectoryScanner>(actor, consumer, completes, ActorOfRepresentation1));
                }
                
                return completes;
            }

            actor.DeadLetters.FailedDelivery(new DeadLetter(actor, ActorOfRepresentation1));
            return null!;
        }

        public ICompletes<Optional<T>> MaybeActorOf<T>(IAddress address)
        {
            if (!actor.IsStopped)
            {
                Action<IDirectoryScanner> consumer = x => x.MaybeActorOf<T>(address);
                var completes = new BasicCompletes<Optional<T>>(actor.Scheduler);
                if (mailbox.IsPreallocated)
                {
                    mailbox.Send(actor, consumer, completes, ActorOfRepresentation2);
                }
                else
                {
                    mailbox.Send(new LocalMessage<IDirectoryScanner>(actor, consumer, completes, ActorOfRepresentation2));
                }

                return completes;
            }

            actor.DeadLetters.FailedDelivery(new DeadLetter(actor, ActorOfRepresentation2));
            return null!;
        }
    }
}
