// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    public sealed class PrivateRootActor : Actor, ISupervisor
    {
        public ISupervisionStrategy SupervisionStrategy { get; }

        public ISupervisor Supervisor { get; } = new DefaultSupervisorImpl();

        public PrivateRootActor()
        {
            SupervisionStrategy = new PrivateRootActorSupervisionStrategy();

            Stage.World.SetPrivateRoot(SelfAs<IStoppable>());

            Stage.ActorProtocolFor<INoProtocol>(
              Definition.Has<PublicRootActor>(Definition.NoParameters, World.PublicRootName),
              this,
              Stage.World.AddressFactory.From(World.PublicRootId, World.PublicRootName),
              null,
              null,
              Logger);

            Stage.ActorProtocolFor<IDeadLetters>(
              Definition.Has<DeadLettersActor>(Definition.NoParameters, World.DeadLettersName),
              this,
              Stage.World.AddressFactory.From(World.DeadLettersId, World.DeadLettersName),
              null,
              null,
              Logger);
        }

        protected internal override void AfterStop()
        {
            Stage.World.SetPrivateRoot(null);
            base.AfterStop();
        }

        public void Inform(Exception error, ISupervised supervised)
        {
            Logger.Log($"PrivateRootActor: Failure of: {supervised.Address}: Stopping.", error);
            supervised.Stop(SupervisionStrategy.Scope);
        }

        private class PrivateRootActorSupervisionStrategy : ISupervisionStrategy
        {
            public int Intensity => 0;

            public long Period => 0;

            public SupervisionStrategyConstants.Scope Scope => SupervisionStrategyConstants.Scope.One;
        }
    }
}
