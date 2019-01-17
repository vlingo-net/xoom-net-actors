// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public interface IAddressFactory
    {
        IAddress FindableBy<T>(T id);
        IAddress From(long reservedId, string name);
        IAddress From(string idString);
        IAddress From(string idString, string name);
        IAddress None();
        IAddress Unique();
        IAddress UniquePrefixedWith(string prefixedWith);
        IAddress UniqueWith(string name);
        IAddress WithHighId();
        IAddress WithHighId(string name);
        long TestNextIdValue();
    }
}
