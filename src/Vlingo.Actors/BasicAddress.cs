// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public sealed class BasicAddress : IAddress
    {
        private readonly long id;
        private readonly string name;

        public long Id => id;

        public long IdSequence => Id;

        public string IdSequenceString => IdString;

        public string IdString => $"{id}";

        public string Name => name ?? id.ToString();

        public bool IsDistributable => false;

        public int CompareTo(IAddress other)
        {
            if (other == null || other.GetType() != typeof(BasicAddress))
            {
                return 1;
            }
            return id.CompareTo(((BasicAddress)other).id);
        }

        public T IdTyped<T>()
            => (T)(object)IdString;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BasicAddress))
            {
                return false;
            }

            return id.Equals(((BasicAddress)obj).id);
        }

        public override int GetHashCode() => id.GetHashCode();

        internal BasicAddress(long reservedId) : this(reservedId, null)
        {
        }

        internal BasicAddress(long reservedId, string name) : this(reservedId, name, false)
        {
        }

        internal BasicAddress(long reservedId, string name, bool prefixName)
        {
            id = reservedId;
            this.name = name == null ? null : (prefixName ? (name + id) : name);
        }
    }
}
