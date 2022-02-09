// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Actors.Plugin.Logging;

namespace Vlingo.Xoom.Actors
{
    public class Logger__Proxy : ILogger
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
        private const string TraceRepresentation17 = "Trace(Vlingo.Xoom.Actors.Logging.LogEvent)";
        private const string DebugRepresentation18 = "Debug(Vlingo.Xoom.Actors.Logging.LogEvent)";
        private const string InfoRepresentation19 = "Info(Vlingo.Xoom.Actors.Logging.LogEvent)";
        private const string WarnRepresentation20 = "Warn(Vlingo.Xoom.Actors.Logging.LogEvent)";
        private const string ErrorRepresentation21 = "Error(Vlingo.Xoom.Actors.Logging.LogEvent)";

        private readonly Actor _actor;
        private readonly IMailbox _mailbox;

        public Logger__Proxy(Actor actor, IMailbox mailbox)
        {
            _actor = actor;
            _mailbox = mailbox;
        }

        public bool IsEnabled => false;
        public string Name => null!;

        public void Close()
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1125253421 = __ => __.Close();
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1125253421, null, CloseRepresentation1);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1125253421, CloseRepresentation1));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, CloseRepresentation1));
            }
        }

        public void Trace(string message)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons751105680 = __ => __.Trace(message);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons751105680, null, TraceRepresentation2);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons751105680, TraceRepresentation2));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, TraceRepresentation2));
            }
        }

        public void Trace(string message, Object[] args)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1516795381 = __ => __.Trace(message, args);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1516795381, null, TraceRepresentation3);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1516795381, TraceRepresentation3));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, TraceRepresentation3));
            }
        }

        public void Trace(string message, Exception exception)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons390017741 = __ => __.Trace(message, exception);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons390017741, null, TraceRepresentation4);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons390017741, TraceRepresentation4));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, TraceRepresentation4));
            }
        }

        public void Debug(string message)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1256278147 = __ => __.Debug(message);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1256278147, null, DebugRepresentation5);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1256278147, DebugRepresentation5));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, DebugRepresentation5));
            }
        }

        public void Debug(string message, Object[] args)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1290725505 = __ => __.Debug(message, args);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1290725505, null, DebugRepresentation6);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1290725505, DebugRepresentation6));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, DebugRepresentation6));
            }
        }

        public void Debug(string message, Exception exception)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1539847183 = __ => __.Debug(message, exception);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1539847183, null, DebugRepresentation7);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1539847183, DebugRepresentation7));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, DebugRepresentation7));
            }
        }

        public void Info(string message)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1473679722 = __ => __.Info(message);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1473679722, null, InfoRepresentation8);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1473679722, InfoRepresentation8));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, InfoRepresentation8));
            }
        }

        public void Info(string message, Object[] args)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons26913663 = __ => __.Info(message, args);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons26913663, null, InfoRepresentation9);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons26913663, InfoRepresentation9));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, InfoRepresentation9));
            }
        }

        public void Info(string message, Exception exception)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons362813474 = __ => __.Info(message, exception);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons362813474, null, InfoRepresentation10);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons362813474, InfoRepresentation10));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, InfoRepresentation10));
            }
        }

        public void Warn(string message)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons503064523 = __ => __.Warn(message);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons503064523, null, WarnRepresentation11);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons503064523, WarnRepresentation11));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, WarnRepresentation11));
            }
        }

        public void Warn(string message, Object[] args)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons393612661 = __ => __.Warn(message, args);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons393612661, null, WarnRepresentation12);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons393612661, WarnRepresentation12));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, WarnRepresentation12));
            }
        }

        public void Warn(string message, Exception exception)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons972588732 = __ => __.Warn(message, exception);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons972588732, null, WarnRepresentation13);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons972588732, WarnRepresentation13));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, WarnRepresentation13));
            }
        }

        public void Error(string message)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1693982355 = __ => __.Error(message);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1693982355, null, ErrorRepresentation14);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1693982355, ErrorRepresentation14));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ErrorRepresentation14));
            }
        }

        public void Error(string message, Object[] args)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons453257971 = __ => __.Error(message, args);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons453257971, null, ErrorRepresentation15);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons453257971, ErrorRepresentation15));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ErrorRepresentation15));
            }
        }

        public void Error(string message, Exception exception)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons57081677 = __ => __.Error(message, exception);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons57081677, null, ErrorRepresentation16);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons57081677, ErrorRepresentation16));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, ErrorRepresentation16));
            }
        }

        public void Trace(LogEvent logEvent)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons180699551 = __ => __.Trace(logEvent);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons180699551, null, TraceRepresentation17);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons180699551, TraceRepresentation17));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, $"{TraceRepresentation17} | {logEvent}"));
            }
        }

        public void Debug(LogEvent logEvent)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1362693295 = __ => __.Debug(logEvent);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1362693295, null, DebugRepresentation18);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1362693295, DebugRepresentation18));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, $"{DebugRepresentation18} | {logEvent}"));
            }
        }

        public void Info(LogEvent logEvent)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1793566457 = __ => __.Info(logEvent);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1793566457, null, InfoRepresentation19);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1793566457, InfoRepresentation19));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, $"{InfoRepresentation19} | {logEvent}"));
            }
        }

        public void Warn(LogEvent logEvent)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1546896550 = __ => __.Warn(logEvent);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1546896550, null, WarnRepresentation20);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1546896550, WarnRepresentation20));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, $"{WarnRepresentation20} | {logEvent}"));
            }
        }

        public void Error(LogEvent logEvent)
        {
            if (!_actor.IsStopped)
            {
                Action<ILogger> cons1762852554 = __ => __.Error(logEvent);
                if (_mailbox.IsPreallocated)
                {
                    _mailbox.Send(_actor, cons1762852554, null, ErrorRepresentation21);
                }
                else
                {
                    _mailbox.Send(
                        new LocalMessage<ILogger>(_actor, cons1762852554, ErrorRepresentation21));
                }
            }
            else
            {
                _actor.DeadLetters?.FailedDelivery(new DeadLetter(_actor, $"{ErrorRepresentation21} | {logEvent}"));
            }
        }
        
        internal void Flush()
        {
            while (_mailbox.PendingMessages > 0)
            {
                _mailbox.Receive()?.Deliver();
            }
        }
    }
}