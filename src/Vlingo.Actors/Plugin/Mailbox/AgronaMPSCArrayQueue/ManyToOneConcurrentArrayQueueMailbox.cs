// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Mailbox.AgronaMPSCArrayQueue
{
    public class ManyToOneConcurrentArrayQueueMailbox : IMailbox, IDisposable
    {
        private bool disposed;
        private readonly IDispatcher dispatcher;
        private readonly BlockingCollection<IMessage> queue;
        private readonly int totalSendRetries;

        internal ManyToOneConcurrentArrayQueueMailbox(
            IDispatcher dispatcher,
            int mailboxSize,
            int totalSendRetries)
        {
            this.dispatcher = dispatcher;
            queue = new BlockingCollection<IMessage>(new ConcurrentQueue<IMessage>(), mailboxSize);
            this.totalSendRetries = totalSendRetries;
        }

        public void Close()
        {
            dispatcher.Close();
            queue.CompleteAdding();
            Dispose(true);
        }
        
        public TaskScheduler TaskScheduler { get; }

        public bool IsClosed => dispatcher.IsClosed;

        public bool IsDelivering
            => throw new NotSupportedException("ManyToOneConcurrentArrayQueueMailbox does not support this operation.");

        public bool IsPreallocated => false;

        public int PendingMessages => queue.Count;

        public bool IsSuspended => false;

        public void Resume(string name) 
            => Console.WriteLine($"WARNING: ManyToOneConcurrentArrayQueueMailbox does not support resume(): {name}");

        public void Run()
            => throw new NotSupportedException("ManyToOneConcurrentArrayQueueMailbox does not support this operation.");

        public void Send(IMessage message)
        {
            for (int tries = 0; tries < totalSendRetries; ++tries)
            {
                if (queue.TryAdd(message))
                {
                    return;
                }
                while (PendingMessages >= queue.BoundedCapacity) ;
            }
            throw new InvalidOperationException("Count not enqueue message due to busy mailbox.");
        }

        public IMessage Receive() => queue.Take();

        public void Send<T>(Actor actor, Action<T> consumer, ICompletes? completes, string representation)
        {
            throw new NotSupportedException("Not a preallocated mailbox.");
        }

        public void SuspendExceptFor(string name, params Type[] overrides)
        {
            if(string.Equals(name, Actors.Mailbox.Stopping))
            {
                Console.WriteLine($"WARNING: ManyToOneConcurrentArrayQueueMailbox does not support SuspendExceptFor(): {name} overrides: {overrides}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;    
            }
      
            if (disposing) {
                
                if (!queue.IsAddingCompleted)
                {
                    Close();
                }

                queue.Dispose();
            }
      
            disposed = true;
        }
    }
}
