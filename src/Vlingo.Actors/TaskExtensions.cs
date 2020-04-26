// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vlingo.Actors
{
    public static class TaskExtensions
    {
        public static Task AndThenTo<T>(this Task<T> task, Action<T> success, Action<Exception>? failure = null)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    failure?.Invoke(t.Exception);
                }

                if (t.IsCompleted)
                {
                    success(t.Result);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        static Task<TResult> ToApm<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith(delegate { callback(task); },
                        CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                }

                return task;
            }

            var tcs = new TaskCompletionSource<TResult>(state);
            task.ContinueWith(delegate
            {
                if (task.IsFaulted)
                {
                    tcs.TrySetException(task.Exception.InnerExceptions);
                }
                else if (task.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(task.Result);
                }

                if (callback != null)
                {
                    callback(tcs.Task);
                }
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}