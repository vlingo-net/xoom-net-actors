using System;

namespace Vlingo.Actors
{
    public sealed class LifeCycle
    {

        public Environment Environment { get; set; }
        
        internal LifeCycle(Environment environment)
        {
            Environment = environment;
        }

        public override int GetHashCode() => Address.GetHashCode();

        internal Address Address => Environment.Address;

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
                Environment.Logger.Log($"vlingo-dotnet/actors: Actor AfterStop() failed: {ex.Message}", ex);
                Environment.Stage.HandleFailureOf<IStoppable>(new StageSupervisedActor<IStoppable>(actor, ex));
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
                Environment.Logger.Log($"vlingo-dotnet/actors: Actor BeforeStart() failed: {ex.Message}");
                Environment.Stage.HandleFailureOf<IStartable>(new StageSupervisedActor<IStartable>(actor, ex));
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
                Environment.Logger.Log($"vlingo-dotnet/actors: Actor AfterRestart() failed: {ex.Message}");
                Environment.Stage.HandleFailureOf<IStartable>(new StageSupervisedActor<IStartable>(actor, ex));
            }
        }

        void BeforeResume<T>(Actor actor, Exception reason)
        {
            try
            {
                actor.BeforeResume(reason);
            }
            catch (Exception ex)
            {
                Environment.Logger.Log($"vlingo-dotnet/actors: Actor BeforeResume() failed: {ex.Message}");
                Environment.Stage.HandleFailureOf<T>(new StageSupervisedActor<T>(actor, ex));
            }
        }

        internal void SendStart(Actor targetActor)
        {
            try
            {
                Action<IStartable> consumer = actor => actor.Start();
                var message = new LocalMessage<IStartable>(targetActor, consumer, "Start()");
                Environment.Mailbox.Send(message);
            }
            catch (Exception ex)
            {
                Environment.Logger.Log("vlingo-dotnet/actors: Actor Start() failed: {ex.Message}");
                Environment.Stage.HandleFailureOf<IStartable>(new StageSupervisedActor<IStartable>(targetActor, ex));
            }
        }

        #endregion

        #region Stowing/Dispersing

        internal bool IsDispersing => Environment.Stowage.IsDispersing;

        internal void DisperseStowedMessages()
        {
            Environment.Stowage.DispersingMode();
            SendFirstIn(Environment.Stowage);
        }

        private void SendFirstIn(Stowage stowage)
        {
            var maybeMessage = stowage.Head;
            if (maybeMessage != null)
            {
                Environment.Mailbox.Send(maybeMessage);
            }
        }

        internal bool IsStowing => Environment.Stowage.IsStowing;

        internal void StowMessages()
        {
            Environment.Stowage.StowingMode();
        }

        #endregion

        #region supervisor/suspending/resuming

        internal bool IsResuming => Environment.Suspended.IsDispersing;

        internal void NextResuming()
        {
            if (IsResuming)
            {
                SendFirstIn(Environment.Suspended);
            }
        }

        internal void Resume()
        {
            Environment.Suspended.DispersingMode();
            SendFirstIn(Environment.Suspended);
        }

        internal bool IsSuspended => Environment.Suspended.IsStowing;

        internal void Suspend()
        {
            Environment.Suspended.StowingMode();
        }

        ISupervisor Supervisor<T>()
        {
            var supervisor = Environment.MaybeSupervisor;

            if (supervisor == null)
            {
                supervisor = Environment.Stage.CommonSupervisorOr(Environment.Stage.World.DefaultSupervisor);
            }

            return supervisor;
        }

        #endregion
    }
}