using System;
using System.Threading.Tasks;
using Vlingo.Common;

namespace Vlingo.Actors.Tests
{
    public class UsesTask__Proxy : IUsesTask
    {
        private const string ChangeRepresentation1 = "Change(int)";
        private const string GetOneRepresentation2 = "GetOne()";
        private const string GetTwoAsyncRepresentation3 = "GetTwoAsync()";

        private readonly Actor actor;
        private readonly IMailbox mailbox;

        public UsesTask__Proxy(Actor actor, IMailbox mailbox)
        {
            this.actor = actor;
            this.mailbox = mailbox;
        }

        public void Change(int newOne)
        {
            if (!actor.IsStopped)
            {
                Action<IUsesTask> cons128873 = __ => __.Change(newOne);
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, cons128873, null, ChangeRepresentation1);
                else
                    mailbox.Send(new LocalMessage<IUsesTask>(actor, cons128873, ChangeRepresentation1));
            }
            else
            {
                actor.DeadLetters.FailedDelivery(new DeadLetter(actor, ChangeRepresentation1));
            }
        }

        public Task<int> GetOne()
        {
            if (!actor.IsStopped)
            {
                var tcs = new TaskCompletionSource<Task<int>>();
                Func<IUsesTask, Task<int>> cons128873 = m =>
                {
                    tcs.SetResult(m.GetOne());
                    return tcs.Task.Unwrap();
                };
                Action<IUsesTask> asyncWrapper = m =>
                {
                    Task Wrap() => cons128873(m);
                    mailbox.SuspendExceptFor(Mailbox.Task, typeof(IAsyncMessage));
                    ExecutorDispatcherAsync.RunTask<IUsesTask>(Wrap, mailbox, actor);
                };
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, asyncWrapper, null, GetOneRepresentation2);
                else
                    mailbox.Send(new LocalMessage<IUsesTask>(actor, asyncWrapper, GetOneRepresentation2));
                return tcs.Task.Unwrap();
            }

            actor.DeadLetters.FailedDelivery(new DeadLetter(actor, GetOneRepresentation2));
            return null;
        }

        public ICompletes<int> GetTwoAsync()
        {
            if (!actor.IsStopped)
            {
                Action<IUsesTask> cons128873 = __ => __.GetTwoAsync();
                var completes = new BasicCompletes<int>(actor.Scheduler);
                if (mailbox.IsPreallocated)
                    mailbox.Send(actor, cons128873, completes, GetTwoAsyncRepresentation3);
                else
                    mailbox.Send(new LocalMessage<IUsesTask>(actor, cons128873, completes, GetTwoAsyncRepresentation3));
                return completes;
            }

            actor.DeadLetters.FailedDelivery(new DeadLetter(actor, GetTwoAsyncRepresentation3));
            return null;
        }
    }
}