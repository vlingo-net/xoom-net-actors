// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors
{
    public sealed class AddressFactory
    {
        private readonly AtomicInteger highId;
        private readonly AtomicInteger nextId;

        internal AddressFactory()
        {
            nextId = new AtomicInteger(1);
            highId = new AtomicInteger(World.HighRootId);
        }

        public Address FindableBy(int id) => new Address(id, id.ToString());

        public Address From(int reservedId, string name) => new Address(reservedId, name);

        public Address Unique() => new Address(nextId.GetAndIncrement());

        public Address UniquePrefixedWith(string prefixedWith) 
            => new Address(nextId.GetAndIncrement(), prefixedWith, true);

        public Address UniqueWith(string name) => new Address(nextId.GetAndIncrement(), name);

        public Address WithHighId(string name) => new Address(highId.DecrementAndGet(), name);

        public Address WithHighId() => WithHighId(null);

        public override string ToString()
            => $"AddressFactory[highId={highId.Get()}, nextId={nextId.Get()}]";

        internal int TestNextIdValue() => nextId.Get();


    }
}
