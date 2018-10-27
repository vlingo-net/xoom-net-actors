// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public interface ICompletesEventually : IStoppable
    {
        void With(object outcome);
    }

    public abstract class CompletesEventually : ICompletesEventually
    {
        public virtual bool IsStopped => false;

        public virtual void Stop()
        {
        }

        public abstract void With(object outcome);
    }
}
