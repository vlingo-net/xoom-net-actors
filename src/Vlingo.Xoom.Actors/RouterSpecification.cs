// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// RouterSpecification specifies the definition and protocol of
    /// the <see cref="Actor"/> to which a <see cref="Router"/> will route,
    /// as well as other details such as pool size.
    /// </summary>
    public class RouterSpecification<T>
    {
        private readonly int _initialPoolSize;
        private readonly Definition _routerDefinition;
        private readonly Type _routerProtocol;

        public RouterSpecification(int poolSize, Definition routerDefinition)
        {
            if (poolSize < 0)
            {
                throw new ArgumentException("poolSize must be 1 or greater");
            }

            _initialPoolSize = poolSize;
            _routerDefinition = routerDefinition;
            _routerProtocol = typeof(T);
        }

        public virtual int InitialPoolSize => _initialPoolSize;

        public virtual Definition RouterDefinition => _routerDefinition;

        public virtual Type RouterProtocol => _routerProtocol;
    }
}
