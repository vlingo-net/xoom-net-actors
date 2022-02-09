// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Actors
{
    internal class DirectoryScannerActor : Actor, IDirectoryScanner
    {
        private readonly Directory _directory;

        public DirectoryScannerActor(Directory directory) => _directory = directory;

        public ICompletes<T> ActorOf<T>(IAddress address)
            => Completes().With(InternalActorOf<T>(address));

        public ICompletes<T> ActorOf<T>(IAddress address, Definition definition)
        {
            var typed = InternalActorOf<T>(address);

            if (typed == null)
            {
                typed = Stage.ActorFor<T>(definition, address);
            }

            return Completes().With(typed);
        }

        public ICompletes<Optional<T>> MaybeActorOf<T>(IAddress address)
        {
            var typed = InternalActorOf<T>(address);
            var maybe = typed == null ? Optional.Empty<T>() : Optional.Of(typed);
            return Completes().With(maybe);
        }

        private T InternalActorOf<T>(IAddress address)
        {
            var actor = _directory.ActorOf(address);
            try
            {
                if (actor != null)
                {
                    return Stage.ActorAs<T>(actor);
                }

                Logger.Debug($"Actor with address: {address} not found; protocol is: {typeof(T).Name}");
            }
            catch(Exception ex)
            {
                Logger.Error($"Error providing protocol: {typeof(T).Name} for actor with address: {address}", ex);
            }

            return default!;
        }
    }
}
