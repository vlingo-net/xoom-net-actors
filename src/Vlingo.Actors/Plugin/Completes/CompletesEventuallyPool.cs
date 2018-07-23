// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors.Plugin.Completes
{
    public class CompletesEventuallyPool : ICompletesEventuallyProvider
    {
        private readonly AtomicLong completesEventuallyId;
        private readonly ICompletesEventually[] pool;
        private readonly AtomicLong poolIndex;
        private readonly long poolSize;
        public CompletesEventuallyPool(int poolSize)
        {
            completesEventuallyId = new AtomicLong(0);
            this.poolSize = poolSize;
            poolIndex = new AtomicLong(0);
            pool = new ICompletesEventually[poolSize];
        }

        public void Close()
        {
            foreach(var completes in pool)
            {
                completes.Stop();
            }
        }

        public ICompletesEventually CompletesEventually
        {
            get
            {
                int index = (int)(poolIndex.IncrementAndGet() % poolSize);
                return pool[index];
            }
        }

        public void InitializeUsing(Stage stage)
        {
            for (var idx = 0; idx < poolSize; ++idx)
            {
                pool[idx] = stage.ActorFor<ICompletesEventually>(Definition.Has<CompletesEventuallyActor>(Definition.NoParameters));
            }
        }

        public ICompletesEventually ProvideCompletesFor<T>(ICompletes<T> clientCompletes)
            => new PooledCompletes<T>(completesEventuallyId.GetAndIncrement(),
                clientCompletes,
                CompletesEventually);
    }
}
