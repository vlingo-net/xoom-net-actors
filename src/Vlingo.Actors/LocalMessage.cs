// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
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
        private Actor? _actor;
        private ICompletes? _completes;
        private Action<TActor>? _consumer;
        private string? _representation;

        public LocalMessage(Actor actor, Action<TActor> consumer, ICompletes? completes, string representation)
        {
            _actor = actor;
            _consumer = consumer;
            _representation = representation;
            _completes = completes;
        }

        public LocalMessage(Actor actor, Action<TActor> consumer, string representation)
            : this(actor, consumer, null, representation)
        {
        }

        public LocalMessage(LocalMessage<TActor> message)
            : this(message._actor!, message._consumer!, message._completes, message._representation!)
        {
        }

        public LocalMessage()
        {
        }

        public virtual Actor Actor => _actor!;

        public virtual void Deliver() => InternalDeliver();

        public Type Protocol => typeof(TActor);

        public virtual bool IsStowed => false;

        public virtual string Representation => _representation!;

        public void Set<TConsumer>(Actor actor, Action<TConsumer> consumer, ICompletes? completes, string representation)
        {
            _actor = actor;
            _consumer = x => consumer.Invoke(x == null ? default : (TConsumer)(object)x);
            _representation = representation;
            _completes = completes;
        }

        public override string ToString() => $"LocalMessage[{_representation}]";

        private void DeadLetter()
        {
            var deadLetter = new DeadLetter(_actor!, _representation!);
            var deadLetters = _actor!.DeadLetters;
            if (deadLetters != null)
            {
                deadLetters.FailedDelivery(deadLetter);
            }
            else
            {
                _actor.Logger.Warn($"vlingo-dotnet/actors: MISSING DEAD LETTERS FOR: {deadLetter}");
            }
        }

        private void InternalDeliver()
        {
            if (_actor!.IsStopped)
            {
                DeadLetter();
            }
            else
            {
                try
                {
                    _actor.CompletesImpl.Reset(_completes);
                    _consumer!.Invoke((TActor)(object)_actor);
                    if (_actor.CompletesImpl.HasInternalOutcomeSet)
                    {
                        // USE THE FOLLOWING. this forces the same ce actor to be used for
                        // all completes outcomes such that completes outcomes cannot be
                        // delivered to the client out of order from the original ordered causes.
                        _actor.LifeCycle.Environment.CompletesEventually(_actor.CompletesImpl).With(_actor.CompletesImpl.InternalOutcome!);

                        // DON'T USE THE FOLLOWING. it selects ce actors in round-robin order which
                        // can easily cause clients to see outcomes of messages delivered later to
                        // an actor before outcomes of messages delivered earlier to the same actor

                        // actor.LifeCycle.Environment.Stage.World.CompletesFor(completes).With(actor.completes.InternalOutcome);
                    }
                }
                catch (Exception ex)
                {
                    // Logging here duplicates logging provided by supervisor.
                    // _actor.Logger.Error($"Message#Deliver(): Exception: {ex.Message} for Actor: {_actor} sending: {_representation}", ex);
                    _actor.Stage.HandleFailureOf(new StageSupervisedActor<TActor>(_actor, ex));
                }
            }
        }
    }
}
