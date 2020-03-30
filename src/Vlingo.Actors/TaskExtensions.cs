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
    }
}