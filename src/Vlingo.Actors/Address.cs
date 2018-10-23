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

        public int Id { get; }
        public string Name { get; }

        internal Address(int reservedId)
            : this(reservedId, null)
        { }

        internal Address(int reservedId, string name)
            : this(reservedId, name, false)
        { }

        internal Address(int reservedId, string name, bool prefixName)
        {
            Id = reservedId;
            Name = name == null ?
                reservedId.ToString() :
                (
                    prefixName ?
                    $"{name}{reservedId}" :
                    name
                );
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