// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    internal sealed class DefaultSupervisionStrategyImpl : ISupervisionStrategy
    {
        public int Intensity => SupervisionStrategyConstants.DefaultIntensity;

        public long Period => SupervisionStrategyConstants.DefaultPeriod;

        public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
    }
}