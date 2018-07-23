// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Common;

namespace Vlingo.Actors
{
    public sealed class Address : IComparable<Address>
    {
        internal static readonly Address None = new Address(0, "None");
        private static AtomicInteger NextId = new AtomicInteger(1);

        public int Id { get; }
        public string Name { get; }

        public static Address From(string name) => new Address(name);

        internal static void Initialize()
        {
            NextId.Set(1);
        }

        internal static Address From(int reservedId, string name) => new Address(reservedId, name);

        internal static int TestNextIdValue() => NextId.Get();

        internal Address(int reservedId, string name)
        {
            Id = reservedId;
            Name = name;
        }

        private Address(string name)
            : this(NextId.GetAndIncrement(), name)
        {
        }

        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != typeof(Address))
            {
                return false;
            }

            return Id == ((Address)obj).Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $"Address[{Id}, Name={Name ?? "(none)"}]";
        }

        public int CompareTo(Address other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}