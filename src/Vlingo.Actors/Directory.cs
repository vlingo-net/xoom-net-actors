// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Vlingo.Actors
{
    internal sealed class Directory
    {
        private readonly IAddress none;
        private readonly ConcurrentDictionary<IAddress, Actor>[] maps;

        internal Directory(IAddress none)
        {
            this.none = none;
            maps = Build();
        }

        internal Actor ActorOf(IAddress address)
        {
            if (maps[MapIndex(address)].TryGetValue(address, out var actor))
            {
                return actor;
            }

            return null;
        }

        internal int Count => maps.Sum(m => m.Count);

        internal void Dump(ILogger logger)
        {
            if (logger.IsEnabled)
            {
                IAddress GetParentAddress(Actor actor) =>
                    actor.LifeCycle.Environment.Parent == null ?
                    none :
                    actor.LifeCycle.Environment.Parent.Address;

                maps
                    .SelectMany(map => map.Values)
                    .Select(actor => $"DIR: DUMP: ACTOR: {actor.Address} PARENT: {GetParentAddress(actor)} TYPE: {actor.GetType().FullName}")
                    .ToList()
                    .ForEach(msg => logger.Log(msg));
            }
        }

        internal bool IsRegistered(IAddress address) => maps[MapIndex(address)].ContainsKey(address);

        internal void Register(IAddress address, Actor actor)
        {
            if (IsRegistered(address))
            {
                throw new InvalidOperationException($"The actor address is already registered: {address}");
            }

            maps[MapIndex(address)].TryAdd(address, actor); // TODO: throw if can't add?
        }

        internal Actor Remove(IAddress address)
        {
            if (maps[MapIndex(address)].TryRemove(address, out Actor actor))
            {
                return actor;
            }
            else
            {
                return null; // TODO: or, throw?
            }
        }

        private ConcurrentDictionary<IAddress, Actor>[] Build()
        {
            // This particular tuning is based on relatively few actors being spread
            // across 32 buckets with only 32 pre-allocated elements, for a total of
            // 1024 actors. This hard-coded configuration will have good performance
            // up to around 75% of 1024 actors, but very average if not poor performance
            // following that.
            //
            // TODO: Change to configuration-based values to enable the
            // application to estimate how many actors are likely to exist at
            // any one time. For example, there will be very few actors in some
            // "applications" such as vlingo/cluster, but then the application
            // running on the cluster itself may have many, many actors. These
            // run on a different stage, and thus should be tuned separately.
            // For example, preallocate 128 buckets that each have a Map of 16K
            // elements in initial capacity (and probably no greater than that).
            // This will support 2 million actors with an average of a few hundred
            // less than 16K actors in each bucket.

            var tempMaps = new ConcurrentDictionary<IAddress, Actor>[32];
            for (int idx = 0; idx < tempMaps.Length; ++idx)
            {
                tempMaps[idx] = new ConcurrentDictionary<IAddress, Actor>(16, 32);  // TODO: base this on scheduler/dispatcher
            }

            return tempMaps;
        }

        private int MapIndex(IAddress address) => Math.Abs(address.GetHashCode() % maps.Length);
    }
}
