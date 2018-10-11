// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public class PooledCompletes : ICompletesEventually
    {
        public long Id { get; }

        public ICompletes ClientCompletes { get; }

        public ICompletesEventually CompletesEventually { get; }

        public PooledCompletes(
            long id,
            ICompletes clientCompletes,
            ICompletesEventually completesEventually)
        {
            Id = id;
            ClientCompletes = clientCompletes;
            CompletesEventually = completesEventually;
        }

        public object Outcome { get; private set; }

        public void With(object outcome)
        {
            Outcome = outcome;
        }

        public bool IsStopped => CompletesEventually.IsStopped;

        public void Stop()
        {
        }
    }
}
