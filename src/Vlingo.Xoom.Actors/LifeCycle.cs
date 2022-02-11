// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors;

internal sealed class LifeCycle
{
    internal Environment Environment { get; set; }
    internal Evictable Evictable { get; set; }

    internal LifeCycle(Environment environment, Evictable evictable)
    {
        Environment = environment;
        Evictable = evictable;
    }

    public override int GetHashCode() => Address.GetHashCode();

    internal IAddress Address => Environment.Address;

    internal Definition Definition
    {
        get
        {
            if (Environment.IsSecured)
            {
                throw new InvalidOperationException("A secured actor cannot provide its definition.");
            }

            return Environment.Definition;
        }
    }

    internal T LookUpProxy<T>() => Environment.LookUpProxy<T>();

    internal bool IsSecured => Environment.IsSecured;

    internal void Secure()
    {
        Environment.SetSecured();
    }

    internal bool IsStopped => Environment.IsStopped;

    internal void Stop(Actor actor)
    {
        Environment.Stop();
        AfterStop(actor);
        Environment.RemoveFromParent(actor);
    }

    #region Standard Lifecycle
    internal void AfterStop(Actor actor)
    {
        try
        {
            actor.AfterStop();
        }
        catch (Exception ex)
        {
            Environment.Logger.Error($"vlingo-net/actors: Actor AfterStop() failed: {ex.Message}", ex);
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<IStoppable>(actor, ex));
        }
    }

    internal void BeforeStart(Actor actor)
    {
        try
        {
            actor.BeforeStart();
        }
        catch (Exception ex)
        {
            Environment.Logger.Error($"vlingo-net/actors: Actor BeforeStart() failed: {ex.Message}");
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<IStartable>(actor, ex));
        }
    }

    internal void AfterRestart(Actor actor, Exception reason)
    {
        try
        {
            actor.AfterRestart(reason);
        }
        catch (Exception ex)
        {
            Environment.Logger.Error($"vlingo-net/actors: Actor AfterRestart() failed: {ex.Message}");
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<IStartable>(actor, ex));
        }
    }

    internal void BeforeRestart<T>(Actor actor, Exception reason)
    {
        try
        {
            actor.BeforeRestart(reason);
        }
        catch (Exception ex)
        {
            Environment.Logger.Error($"vlingo-net/actors: Actor BeforeRestart() failed: {ex.Message}");
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<T>(actor, ex));
        }
    }

    internal void BeforeResume<T>(Actor actor, Exception reason)
    {
        try
        {
            actor.BeforeResume(reason);
        }
        catch (Exception ex)
        {
            Environment.Logger.Error($"vlingo-net/actors: Actor BeforeResume() failed: {ex.Message}");
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<T>(actor, ex));
        }
    }

    internal void SendStart(Actor targetActor)
    {
        try
        {
            Action<IStartable> consumer = x => x.Start();
            if (!Environment.Mailbox.IsPreallocated)
            {
                var message = new LocalMessage<IStartable>(targetActor, consumer, "Start()");
                Environment.Mailbox.Send(message);
            }
            else
            {
                Environment.Mailbox.Send(targetActor, consumer, null, "Start()");
            }
        }
        catch (Exception ex)
        {
            Environment.Logger.Error("vlingo-net/actors: Actor Start() failed: {ex.Message}");
            Environment.Stage.HandleFailureOf(new StageSupervisedActor<IStartable>(targetActor, ex));
        }
    }

    #endregion

    #region supervisor/suspending/resuming

    internal void Resume() => Environment.Mailbox.Resume(Mailbox.Exceptional);

    internal bool IsSuspended => Environment.Mailbox.IsSuspended;

    internal void Suspend() => Environment.Mailbox.SuspendExceptFor(Mailbox.Exceptional, typeof(IStoppable));

    internal void SuspendForStop() => Environment.Mailbox.SuspendExceptFor(Mailbox.Stopping, typeof(IStoppable));

    internal ISupervisor Supervisor<T>()
    {
        var supervisor = Environment.MaybeSupervisor;

        if (supervisor == null)
        {
            supervisor = Environment.Stage.CommonSupervisorOr<T>(Environment.Stage.World.DefaultSupervisor);
        }

        return supervisor;
    }

    #endregion
}