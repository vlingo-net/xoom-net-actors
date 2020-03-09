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
    public class LocalMessage<TActor> : IMessage
    {
        private Actor? actor;
        private ICompletes? completes;
        private Action<TActor>? consumer;
        private string? representation;

        public LocalMessage(Actor actor, Action<TActor> consumer, ICompletes? completes, string representation)
        {
            this.actor = actor;
            this.consumer = consumer;
            this.representation = representation;
            this.completes = completes;
        }

        public LocalMessage(Actor actor, Action<TActor> consumer, string representation)
            : this(actor, consumer, null, representation)
        {
        }

        public LocalMessage(LocalMessage<TActor> message)
            : this(message.actor!, message.consumer!, message.completes, message.representation!)
        {
        }

        public LocalMessage()
        {
        }

        public virtual Actor Actor => actor!;

        public virtual void Deliver() => InternalDeliver();

        public Type Protocol => typeof(TActor);

        public virtual bool IsStowed => false;

        public virtual string Representation => representation!;

        public void Set<TConsumer>(Actor actor, Action<TConsumer> consumer, ICompletes? completes, string representation)
        {
            this.actor = actor;
            this.consumer = x => consumer.Invoke(x == null ? default : (TConsumer)(object)x);
            this.representation = representation;
            this.completes = completes;
        }

        public override string ToString() => $"LocalMessage[{representation}]";

        private void DeadLetter()
        {
            var deadLetter = new DeadLetter(actor!, representation!);
            var deadLetters = actor!.DeadLetters;
            if (deadLetters != null)
            {
                deadLetters.FailedDelivery(deadLetter);
            }
            else
            {
                actor.Logger.Warn($"vlingo-dotnet/actors: MISSING DEAD LETTERS FOR: {deadLetter}");
            }
        }

        private void InternalDeliver()
        {
            if (actor!.IsStopped)
            {
                DeadLetter();
            }
            else
            {
                try
                {
                    actor.CompletesImpl.Reset(completes);
                    consumer!.Invoke((TActor)(object)actor);
                    if (actor.CompletesImpl.HasInternalOutcomeSet)
                    {
                        // USE THE FOLLOWING. this forces the same ce actor to be used for
                        // all completes outcomes such that completes outcomes cannot be
                        // delivered to the client out of order from the original ordered causes.
                        actor.LifeCycle.Environment.CompletesEventually(actor.CompletesImpl).With(actor.CompletesImpl.InternalOutcome!);

                        // DON'T USE THE FOLLOWING. it selects ce actors in round-robin order which
                        // can easily cause clients to see outcomes of messages delivered later to
                        // an actor before outcomes of messages delivered earlier to the same actor

                        // actor.LifeCycle.Environment.Stage.World.CompletesFor(completes).With(actor.completes.InternalOutcome);
                    }
                }
                catch(Exception ex)
                {
                    actor.Logger.Error($"Message#Deliver(): Exception: {ex.Message} for Actor: {actor} sending: {representation}", ex);
                    actor.Stage.HandleFailureOf(new StageSupervisedActor<TActor>(actor, ex));
                }
            }
        }
    }
}
