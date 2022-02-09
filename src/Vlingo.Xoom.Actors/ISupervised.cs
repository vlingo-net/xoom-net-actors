// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using static Vlingo.Xoom.Actors.SupervisionStrategyConstants;

namespace Vlingo.Xoom.Actors
{
    public interface ISupervised
    {
        IAddress Address { get; }
        void Escalate();
        void RestartWithin(long period, int intensity, Scope scope);
        void Resume();
        void Stop(Scope scope);
        ISupervisor Supervisor { get; }
        void Suspend();
        Exception Error { get; }
    }
}