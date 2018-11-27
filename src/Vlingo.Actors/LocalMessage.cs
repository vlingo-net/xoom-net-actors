// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class LocalMessage<T> : IMessage
    {
        private Actor actor;
        private ICompletes<object> completes;
        private Action<T> consumer;
        private string representation;

        public LocalMessage(Actor actor, Action<T> consumer, ICompletes<object> completes, string representation)
        {
            this.actor = actor;
            this.consumer = consumer;
            this.representation = representation;
            this.completes = completes;
        }

        public LocalMessage(Actor actor, Action<T> consumer, string representation)
            : this(actor, consumer, null, representation)
        {
        }

        public LocalMessage(LocalMessage<T> message)
            : this(message.actor, message.consumer, message.completes, message.representation)
        {
        }

        public LocalMessage(IMailbox mailbox)
        {
        }

        public virtual Actor Actor => actor;

        public virtual void Deliver()
        {
            if (actor.LifeCycle.IsResuming)
            {
                if (IsStowed)
                {
                    InternalDeliver(this);
                }
                else
                {
                    InternalDeliver(actor.LifeCycle.Environment.Suspended.SwapWith<T>(this));
                }
                actor.LifeCycle.NextResuming();
            }
            else if (actor.IsDispersing)
            {
                InternalDeliver(this);
                actor.LifeCycle.NextDispersing();
            }
            else
            {
                InternalDeliver(this);
            }
        }

        public virtual bool IsStowed => false;

        public virtual string Representation => representation;

        public void Set(Actor actor, Action<object> consumer, ICompletes<object> completes, string representation)
        {
            this.actor = actor;
            this.consumer = (Action<T>)(object)consumer;
            this.representation = representation;
            this.completes = completes;
        }

        public override string ToString() => $"LocalMessage[{representation}]";

        private void DeadLetter()
        {
            var deadLetter = new DeadLetter(actor, representation);
            var deadLetters = actor.DeadLetters;
            if(deadLetters != null)
            {
                deadLetters.FailedDelivery(deadLetter);
            }
            else
            {
                actor.Logger.Log($"vlingo-dotnet/actors: MISSING DEAD LETTERS FOR: {deadLetter}");
            }
        }

        private void InternalDeliver(IMessage message)
        {
            var protocol = typeof(T);

            if (actor.IsStopped)
            {
                DeadLetter();
            }
            else if (actor.LifeCycle.IsSuspended)
            {
                actor.LifeCycle.Environment.Suspended.Stow<T>(message);
            }
            else if (actor.IsStowing && !actor.LifeCycle.Environment.IsStowageOverride(protocol))
            {
                actor.LifeCycle.Environment.Stowage.Stow<T>(message);
            }
            else
            {
                try
                {
                    actor.completes.Reset(completes);
                    consumer.Invoke(actor);
                    if (actor.completes.__internal__outcomeSet)
                    {
                        var outcome = actor.completes.Outcome;
                        actor.LifeCycle.Environment.Stage.World.CompletesFor(completes).With(actor.completes.__internal__outcome);
                    }
                }
                catch(Exception ex)
                {
                    actor.Logger.Log($"Message#Deliver(): Exception: {ex.Message} for Actor: {actor} sending: {representation}", ex);
                    actor.Stage.HandleFailureOf<T>(new StageSupervisedActor<T>(actor, ex));
                }
            }
        }
    }
}
