// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    public class PooledCompletes : ICompletesEventually
    {
        public long Id { get; }

        public ICompletes? ClientCompletes { get; }

        public ICompletesEventually CompletesEventually { get; }

        public PooledCompletes(
            long id,
            ICompletes? clientCompletes,
            ICompletesEventually completesEventually)
        {
            Id = id;
            ClientCompletes = clientCompletes;
            CompletesEventually = completesEventually;
        }

        public virtual object? Outcome { get; private set; }

        public virtual void With(object? outcome)
        {
            Outcome = outcome;
            CompletesEventually.With(this);
        }

        public virtual bool IsStopped => CompletesEventually.IsStopped;

        public virtual IAddress Address => CompletesEventually.Address;

        public virtual void Stop()
        {
        }

        public override int GetHashCode() => 31 * Address.GetHashCode();

        public override bool Equals(object? other)
        {
            if (other == null || other.GetType() != GetType())
            {
                return false;
            }

            return Address.Equals(((PooledCompletes)other).Address);
        }

        public override string ToString() => $"PooledCompletes[Id={Id} Address={Address} Outcome={Outcome} IsStopped={IsStopped}]";

        public void Conclude()
        {
        }
    }
}
