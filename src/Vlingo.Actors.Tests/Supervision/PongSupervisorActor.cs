// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors.Tests.Supervision
{
    public class PongSupervisorActor : Actor, ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy => throw new NotImplementedException();

        public ISupervisor Supervisor => throw new NotImplementedException();

        public void Inform(Exception error, ISupervised supervised)
        {
            throw new NotImplementedException();
        }
    }
}
