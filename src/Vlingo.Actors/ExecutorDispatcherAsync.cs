// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Actors
{
    internal interface IAsyncMessage {}
    
    public class ExecutorDispatcherAsync : TaskScheduler
    {
        private readonly IMailbox _mailbox;

        public ExecutorDispatcherAsync(IMailbox mailbox)
        {
            _mailbox = mailbox;
        }

        public override int MaximumConcurrencyLevel { get; } = 1;

        protected override IEnumerable<Task> GetScheduledTasks() => null;

        protected override void QueueTask(Task task)
        {
            if ((task.CreationOptions & TaskCreationOptions.LongRunning) == TaskCreationOptions.LongRunning)
            {
                // Executing a LongRunning task is bad practice, it will potentially hang the actor and starve the ThreadPool
                // Scheduling a task to not execute it inline but on thread pool.
                ScheduleTask(task);
                return;
            }

            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false; // executing already inline through actors queue

        public static void RunTask<T>(Func<Task> asyncHandler, IMailbox mailbox, Actor actor)
        {
            Task<Task>.Factory.StartNew(asyncHandler, CancellationToken.None, TaskCreationOptions.None, mailbox.TaskScheduler)
                .Unwrap()
                .ContinueWith(parent =>
                {
                    var exception = ExceptionFor(parent);
                    if (exception == null)
                    {
                        mailbox.Resume(Mailbox.Task);
                    }
                    else
                    {
                        actor.Stage.HandleFailureOf(new StageSupervisedActor<T>(actor, exception));
                    }
                }, mailbox.TaskScheduler);
        }
        
        private void ScheduleTask(Task task)
        {            
            //_mailbox.Send(new LocalMessage<>())
        }
        
        private static Exception? ExceptionFor(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.Canceled:
                    return new TaskCanceledException();

                case TaskStatus.Faulted:
                    return UnwrapAggregateException(task.Exception);
            }

            return null;
        }
        
        private static Exception UnwrapAggregateException(AggregateException aggregateException)
        {
            if (aggregateException == null)
            {
                return null;
            }

            if (aggregateException.InnerExceptions.Count == 1)
            {
                return aggregateException.InnerExceptions[0];
            }

            return aggregateException;
        }
    }
}