// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Common;

namespace Vlingo.Actors
{
    internal class DirectoryScannerActor : Actor, IDirectoryScanner
    {
        private readonly Directory directory;

        public DirectoryScannerActor(Directory directory)
        {
            this.directory = directory;
        }

        public ICompletes<T> ActorOf<T>(IAddress address)
        {
            var actor = directory.ActorOf(address);
            if(actor != null)
            {
                return Completes<T>().With(Stage.ActorAs<T>(actor));
            }

            return Completes<T>().With(default(T));
        }
    }
}
