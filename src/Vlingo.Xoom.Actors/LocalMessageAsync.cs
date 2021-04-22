// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace Vlingo.Xoom.Actors
{
    public class LocalMessageAsync : LocalMessage<IAsyncMessage>
    {
        private readonly ExecutorDispatcherAsync _executor;
        private readonly Task _task;

        public LocalMessageAsync(ExecutorDispatcherAsync executor, Task task)
        {
            _executor = executor;
            _task = task;
        }

        public override void Deliver() => _executor.ExecuteTask(_task);
    }
}