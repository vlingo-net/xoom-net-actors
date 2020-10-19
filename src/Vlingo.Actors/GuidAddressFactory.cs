// Copyright © 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;
using Vlingo.Common.Identity;
using Vlingo.UUID;

namespace Vlingo.Actors
{
    public class GuidAddressFactory : IAddressFactory
    {
        private static IAddress Empty = new GuidAddress(Guid.Empty, "(Empty)");
        
        private readonly IIdentityGenerator _generator;
        private IdentityGeneratorType _type;

        private AtomicLong _highId;
        
        public GuidAddressFactory(IdentityGeneratorType type)
        {
            _type = type;
            _generator = type.Generator();
            _highId = new AtomicLong(World.HighRootId);
        }

        public virtual IAddress FindableBy<T>(T id) => 
            Guid.TryParse(id!.ToString(), out var parsed) ? 
                new GuidAddress(parsed) :
                new GuidAddress(long.Parse(id!.ToString()!).ToGuid());

        public virtual IAddress From(long reservedId, string name) =>
            Guid.TryParse(reservedId!.ToString(), out var parsed) ? 
                new GuidAddress(parsed, name) :
                new GuidAddress(long.Parse(reservedId!.ToString()).ToGuid(), name);

        public virtual IAddress From(string idString) =>
            Guid.TryParse(idString, out var parsed) ? 
                new GuidAddress(parsed) :
                new GuidAddress(long.Parse(idString).ToGuid());

        public virtual IAddress From(string idString, string name) =>
            Guid.TryParse(idString, out var parsed) ? 
                new GuidAddress(parsed, name) :
                new GuidAddress(long.Parse(idString).ToGuid(), name);

        public virtual IAddress None() => Empty;

        public IAddress Unique() => new GuidAddress(_generator.Generate());

        public IAddress UniquePrefixedWith(string prefixedWith) => new GuidAddress(_generator.Generate(), prefixedWith, true);

        public IAddress UniqueWith(string? name) => new GuidAddress(_generator.Generate(name!), name);

        public virtual IAddress WithHighId() => WithHighId(null);

        public virtual IAddress WithHighId(string? name) => new GuidAddress(GuidFrom(_highId.DecrementAndGet()), name);

        public long TestNextIdValue() => throw new NotImplementedException($"Unsupported for {GetType().Name}.");
        
        protected Guid GuidFrom(long id) => id.ToGuid();

        protected Guid Unique(string name)
        {
            var found = false;

            var highest = _highId.Get();

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (!found)
            {
                var guid = name == null ? _generator.Generate() : _generator.Generate(name);

                var lsb = guid.ToLeastSignificantBits();

                // assume that these are the N special reserved ids
                if (lsb > World.HighRootId)
                {
                    return guid;
                }

                if (lsb < highest)
                {
                    return guid;
                }
            }

            throw new InvalidOperationException("Cannot allocate unique address id.");
        }
    }
}