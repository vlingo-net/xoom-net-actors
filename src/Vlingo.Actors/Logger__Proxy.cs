using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vlingo.Actors;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public class Logger__Proxy : Vlingo.Actors.ILogger
    {
        private const string CloseRepresentation1 = "Close()";
        private const string TraceRepresentation2 = "Trace(string)";
        private const string TraceRepresentation3 = "Trace(string, System.Object[])";
        private const string TraceRepresentation4 = "Trace(string, System.Exception)";
        private const string DebugRepresentation5 = "Debug(string)";
        private const string DebugRepresentation6 = "Debug(string, System.Object[])";
        private const string DebugRepresentation7 = "Debug(string, System.Exception)";
        private const string InfoRepresentation8 = "Info(string)";
        private const string InfoRepresentation9 = "Info(string, System.Object[])";
        private const string InfoRepresentation10 = "Info(string, System.Exception)";
        private const string WarnRepresentation11 = "Warn(string)";
        private const string WarnRepresentation12 = "Warn(string, System.Object[])";
        private const string WarnRepresentation13 = "Warn(string, System.Exception)";
        private const string ErrorRepresentation14 = "Error(string)";
        private const string ErrorRepresentation15 = "Error(string, System.Object[])";
        private const string ErrorRepresentation16 = "Error(string, System.Exception)";
        private const string TraceRepresentation17 = "Trace(Vlingo.Actors.Logging.LogEvent)";
        private const string DebugRepresentation18 = "Debug(Vlingo.Actors.Logging.LogEvent)";
        private const string InfoRepresentation19 = "Info(Vlingo.Actors.Logging.LogEvent)";
        private const string WarnRepresentation20 = "Warn(Vlingo.Actors.Logging.LogEvent)";
        private const string ErrorRepresentation21 = "Error(Vlingo.Actors.Logging.LogEvent)";

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
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1125253421 = __ => __.Close();
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1125253421, null, CloseRepresentation1);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1125253421, CloseRepresentation1));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, CloseRepresentation1));
            }
        }

        public void Trace(string message)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons751105680 = __ => __.Trace(message);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons751105680, null, TraceRepresentation2);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons751105680, TraceRepresentation2));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, TraceRepresentation2));
            }
        }

        public void Trace(string message, System.Object[] args)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1516795381 = __ => __.Trace(message, args);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1516795381, null, TraceRepresentation3);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1516795381, TraceRepresentation3));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, TraceRepresentation3));
            }
        }

        public void Trace(string message, System.Exception exception)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons390017741 = __ => __.Trace(message, exception);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons390017741, null, TraceRepresentation4);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons390017741, TraceRepresentation4));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, TraceRepresentation4));
            }
        }

        public void Debug(string message)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1256278147 = __ => __.Debug(message);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1256278147, null, DebugRepresentation5);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1256278147, DebugRepresentation5));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, DebugRepresentation5));
            }
        }

        public void Debug(string message, System.Object[] args)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1290725505 = __ => __.Debug(message, args);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1290725505, null, DebugRepresentation6);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1290725505, DebugRepresentation6));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, DebugRepresentation6));
            }
        }

        public void Debug(string message, System.Exception exception)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1539847183 = __ => __.Debug(message, exception);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1539847183, null, DebugRepresentation7);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1539847183, DebugRepresentation7));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, DebugRepresentation7));
            }
        }

        public void Info(string message)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1473679722 = __ => __.Info(message);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1473679722, null, InfoRepresentation8);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1473679722, InfoRepresentation8));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, InfoRepresentation8));
            }
        }

        public void Info(string message, System.Object[] args)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons26913663 = __ => __.Info(message, args);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons26913663, null, InfoRepresentation9);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons26913663, InfoRepresentation9));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, InfoRepresentation9));
            }
        }

        public void Info(string message, System.Exception exception)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons362813474 = __ => __.Info(message, exception);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons362813474, null, InfoRepresentation10);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons362813474, InfoRepresentation10));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, InfoRepresentation10));
            }
        }

        public void Warn(string message)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons503064523 = __ => __.Warn(message);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons503064523, null, WarnRepresentation11);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons503064523, WarnRepresentation11));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, WarnRepresentation11));
            }
        }

        public void Warn(string message, System.Object[] args)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons393612661 = __ => __.Warn(message, args);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons393612661, null, WarnRepresentation12);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons393612661, WarnRepresentation12));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, WarnRepresentation12));
            }
        }

        public void Warn(string message, System.Exception exception)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons972588732 = __ => __.Warn(message, exception);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons972588732, null, WarnRepresentation13);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons972588732, WarnRepresentation13));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, WarnRepresentation13));
            }
        }

        public void Error(string message)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1693982355 = __ => __.Error(message);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1693982355, null, ErrorRepresentation14);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1693982355, ErrorRepresentation14));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ErrorRepresentation14));
            }
        }

        public void Error(string message, System.Object[] args)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons453257971 = __ => __.Error(message, args);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons453257971, null, ErrorRepresentation15);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons453257971, ErrorRepresentation15));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ErrorRepresentation15));
            }
        }

        public void Error(string message, System.Exception exception)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons57081677 = __ => __.Error(message, exception);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons57081677, null, ErrorRepresentation16);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons57081677, ErrorRepresentation16));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, ErrorRepresentation16));
            }
        }

        public void Trace(Vlingo.Actors.Logging.LogEvent logEvent)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons180699551 = __ => __.Trace(logEvent);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons180699551, null, TraceRepresentation17);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons180699551, TraceRepresentation17));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, $"{TraceRepresentation17} | {logEvent}"));
            }
        }

        public void Debug(Vlingo.Actors.Logging.LogEvent logEvent)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1362693295 = __ => __.Debug(logEvent);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1362693295, null, DebugRepresentation18);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1362693295, DebugRepresentation18));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, $"{DebugRepresentation18} | {logEvent}"));
            }
        }

        public void Info(Vlingo.Actors.Logging.LogEvent logEvent)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1793566457 = __ => __.Info(logEvent);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1793566457, null, InfoRepresentation19);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1793566457, InfoRepresentation19));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, $"{InfoRepresentation19} | {logEvent}"));
            }
        }

        public void Warn(Vlingo.Actors.Logging.LogEvent logEvent)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1546896550 = __ => __.Warn(logEvent);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1546896550, null, WarnRepresentation20);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1546896550, WarnRepresentation20));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, $"{WarnRepresentation20} | {logEvent}"));
            }
        }

        public void Error(Vlingo.Actors.Logging.LogEvent logEvent)
        {
            if (!this.actor.IsStopped)
            {
                Action<Vlingo.Actors.ILogger> cons1762852554 = __ => __.Error(logEvent);
                if (this.mailbox.IsPreallocated)
                {
                    this.mailbox.Send(this.actor, cons1762852554, null, ErrorRepresentation21);
                }
                else
                {
                    this.mailbox.Send(
                        new LocalMessage<Vlingo.Actors.ILogger>(this.actor, cons1762852554, ErrorRepresentation21));
                }
            }
            else
            {
                this.actor.DeadLetters.FailedDelivery(new DeadLetter(this.actor, $"{ErrorRepresentation21} | {logEvent}"));
            }
        }
        
        internal void Flush()
        {
            while (mailbox.PendingMessages > 0)
            {
                mailbox.Receive()?.Deliver();
            }
        }
    }
}