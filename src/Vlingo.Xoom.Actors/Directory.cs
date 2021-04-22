// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vlingo.Actors
{
    internal sealed class Directory
    {
        private readonly IAddress _none;
        private readonly ConcurrentDictionary<IAddress, Actor>[] _maps;
        
        // (1) Configuration: 32, 32; used in default Stage
        // This default tuning manages few actors being spread
        // across 32 buckets with only 32 pre-allocated elements, for a total of
        // 1024 actors. This hard-coded configuration will have good performance
        // up to around 75% of 1024 actors, but very average if not poor performance
        // following that.
        //
        // (2) Configuration: 128, 16,384; used by Grid
        // This tuning enables millions of actors at any one time.
        // For example, there will be very few actors in some
        // "applications" such as vlingo/cluster, but then the application
        // running on the cluster itself may have many, many actors. These
        // run on a different stage, and thus should be tuned separately.
        // For example, preallocate 128 buckets that each have a Map of 16K
        // elements in initial capacity (and probably no greater than that).
        // This will support 2 million actors with an average of a few hundred
        // less than 16K actors in each bucket.
        
        private readonly int _buckets;
        private readonly int _initialCapacity;

        // TODO: base this on scheduler/dispatcher
        private const int ConcurrencyLevel = 16;

        internal Directory(IAddress none, int buckets, int initialCapacity)
        {
            _none = none;
            _buckets = buckets;
            _initialCapacity = initialCapacity;
            _maps = Build();
        }

        internal Actor? ActorOf(IAddress? address)
        {
            if (address != null && _maps[MapIndex(address)].TryGetValue(address, out var actor))
            {
                return actor;
            }

            return null;
        }

        internal int Count => _maps.Sum(m => m.Count);

        internal void Dump(ILogger logger)
        {
            if (logger.IsEnabled)
            {
                IAddress GetParentAddress(Actor actor) =>
                    actor.LifeCycle.Environment.Parent == null ?
                    _none :
                    actor.LifeCycle.Environment.Parent.Address;

                _maps
                    .SelectMany(map => map.Values)
                    .Select(actor => $"DIR: DUMP: ACTOR: {actor.Address} PARENT: {GetParentAddress(actor)} TYPE: {actor.GetType().FullName}")
                    .ToList()
                    .ForEach(msg => logger.Debug(msg));
            }
        }

        internal bool IsRegistered(IAddress address) => _maps[MapIndex(address)].ContainsKey(address);

        internal void Register(IAddress address, Actor actor)
        {
            if (IsRegistered(address))
            {
                throw new ActorAddressAlreadyRegisteredException(actor, address);
            }

            _maps[MapIndex(address)].TryAdd(address, actor); // TODO: throw if can't add?
        }

        internal Actor Remove(IAddress address)
        {
            if (_maps[MapIndex(address)].TryRemove(address, out Actor? actor))
            {
                return actor;
            }

            return null!; // TODO: or, throw?
        }

        internal IEnumerable<Actor> EvictionCandidates(long thresholdMillis) =>
            _maps.SelectMany(m => m.Values).Where(a =>
                a.LifeCycle.Evictable.IsStale(thresholdMillis) && a.LifeCycle.Environment.Mailbox.PendingMessages == 0);

        internal IEnumerable<IAddress> Addresses => _maps.SelectMany(m => m.Keys);

        private ConcurrentDictionary<IAddress, Actor>[] Build()
        {
            var tempMaps = new ConcurrentDictionary<IAddress, Actor>[_buckets];
            for (var idx = 0; idx < tempMaps.Length; ++idx)
            {
                tempMaps[idx] = new ConcurrentDictionary<IAddress, Actor>(ConcurrencyLevel, _initialCapacity);  // TODO: base this on scheduler/dispatcher
            }

            return tempMaps;
        }

        private int MapIndex(IAddress address) => Math.Abs(address.GetHashCode() % _maps.Length);
    }
}
