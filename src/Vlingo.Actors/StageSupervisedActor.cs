// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Text;

namespace Vlingo.Actors
{
    public class StageSupervisedActor<T> : ISupervised
    {
        public StageSupervisedActor(Actor actor, Exception ex)
        {

        }

        public Address Address => throw new NotImplementedException();

        public ISupervisor Supervisor => throw new NotImplementedException();

        public Exception Error => throw new NotImplementedException();

        public void Escalate()
        {
            throw new NotImplementedException();
        }

        public void RestartWithin(long period, int intensity, SupervisionStrategyConstants.Scope scope)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public void Stop(SupervisionStrategyConstants.Scope scope)
        {
            throw new NotImplementedException();
        }

        public void Suspend()
        {
            throw new NotImplementedException();
        }
    }
}
