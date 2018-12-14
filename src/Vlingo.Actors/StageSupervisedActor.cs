// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using static Vlingo.Actors.SupervisionStrategyConstants;

namespace Vlingo.Actors
{
    public class StageSupervisedActor<T> : ISupervised
    {
        private readonly Actor actor;

        protected internal StageSupervisedActor(Actor actor, Exception error)
        {
            this.actor = actor;
            Error = error;
        }

        public virtual IAddress Address => actor.Address;

        public virtual void Escalate() => Supervisor.Supervisor.Inform(Error, this);

        public virtual void RestartWithin(long period, int intensity, Scope scope)
        {
            if (FailureThresholdReached(period, intensity))
            {
                Stop(scope);
            }
            else
            {
                if (scope == Scope.One)
                {
                    RestartWithin(actor, period, intensity);
                }
                else
                {
                    foreach (var actor in SelfWithSiblings())
                    {
                        RestartWithin(actor, period, intensity);
                    }
                }
            }
        }

        public virtual void Resume()
        {
            actor.LifeCycle.BeforeResume<T>(actor, Error);
            actor.LifeCycle.Resume();
        }

        public virtual void Stop(Scope scope)
        {
            if(scope == Scope.One)
            {
                actor.Stop();
            }
            else
            {
                foreach(var actor in SelfWithSiblings())
                {
                    actor.Stop();
                }
            }
        }

        public virtual void Suspend() => actor.LifeCycle.Suspend();

        public virtual ISupervisor Supervisor => actor.LifeCycle.Supervisor<T>();

        public virtual Exception Error { get; }

        private IEnumerable<Actor> SelfWithSiblings()
            => EnvironmentOf(EnvironmentOf(actor).Parent).Children;

        private static Environment EnvironmentOf(Actor actor) => actor.LifeCycle.Environment;

        private bool FailureThresholdReached(long period, int intensity)
            => EnvironmentOf(actor).FailureMark.FailedWithExcessiveFailures(period, intensity);

        private void RestartWithin(Actor actor, long period, int intensity)
        {
            actor.LifeCycle.BeforeRestart<T>(actor, Error);
            // TODO: Actually restart actor here? I am not
            // yet convinced that it is necessary or practical.
            // Please convince me.
            actor.LifeCycle.AfterRestart(actor, Error);
            Resume();
        }
    }
}
