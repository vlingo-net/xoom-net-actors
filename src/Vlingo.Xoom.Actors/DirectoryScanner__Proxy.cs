// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    public class DirectoryScanner__Proxy : IDirectoryScanner
    {
        private const string ActorOfRepresentation1 = "ActorOf<T>(Vlingo.Xoom.Actors.Address)";
        private const string ActorOfRepresentation2 = "ActorOf<T>(Vlingo.Xoom.Actors.Address, Vlingo.Xoom.Actors.Definition definition)";
        private const string ActorOfRepresentation3 = "MaybeActorOf<T>(Vlingo.Xoom.Actors.Address)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public DirectoryScanner__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public ICompletes<T> ActorOf<T>(IAddress address)
        {
            if (!_actor.IsStopped)
            {
                Action<IDirectoryScanner> consumer = x => x.ActorOf<T>(address);
                var completes = new BasicCompletes<T>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, completes, ActorOfRepresentation1);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDirectoryScanner>(_actor, consumer, completes, ActorOfRepresentation1));
                }
                
                return completes;
            }

            _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ActorOfRepresentation1));
            return null!;
        }

        public ICompletes<T> ActorOf<T>(IAddress address, Definition definition)
        {
            if (!_actor.IsStopped)
            {
                Action<IDirectoryScanner> consumer = x => x.ActorOf<T>(address);
                var completes = new BasicCompletes<T>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, completes, ActorOfRepresentation2);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDirectoryScanner>(_actor, consumer, completes, ActorOfRepresentation2));
                }
                
                return completes;
            }

            _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ActorOfRepresentation2));
            return null!;
        }

        public ICompletes<Optional<T>> MaybeActorOf<T>(IAddress address)
        {
            if (!_actor.IsStopped)
            {
                Action<IDirectoryScanner> consumer = x => x.MaybeActorOf<T>(address);
                var completes = new BasicCompletes<Optional<T>>(_actor.Scheduler);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, consumer, completes, ActorOfRepresentation3);
                }
                else
                {
                    _mailbox.Send(new LocalMessage<IDirectoryScanner>(_actor, consumer, completes, ActorOfRepresentation3));
                }

                return completes;
            }

            _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ActorOfRepresentation3));
            return null!;
        }
    }
}
