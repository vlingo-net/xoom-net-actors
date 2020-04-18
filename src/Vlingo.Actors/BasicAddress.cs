// Copyright (c) 2012-2020 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public sealed class BasicAddress : IAddress
    {
        private readonly long _id;
        private readonly string? _name;

        public long Id => _id;

        public long IdSequence => Id;

        public string IdSequenceString => IdString;

        public string IdString => $"{_id}";

        public string Name => _name ?? _id.ToString();

        public bool IsDistributable => false;

        public int CompareTo(IAddress other)
        {
            if (other == null || other.GetType() != typeof(BasicAddress))
            {
                return 1;
            }
            return _id.CompareTo(((BasicAddress)other)._id);
        }

        public T IdTyped<T>()
            => (T)(object)IdString;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(BasicAddress))
            {
                return false;
            }

            return _id.Equals(((BasicAddress)obj)._id);
        }

        public override int GetHashCode() => _id.GetHashCode();

        public override string ToString() => $"Address[Id={_id}, Name={_name ?? "(none)"}]";

        internal BasicAddress(long reservedId) : this(reservedId, null)
        {
        }

        internal BasicAddress(long reservedId, string? name) : this(reservedId, name, false)
        {
        }

        internal BasicAddress(long reservedId, string? name, bool prefixName)
        {
            _id = reservedId;
            this._name = name == null ? null : prefixName ? name + _id : name;
        }
    }
}
