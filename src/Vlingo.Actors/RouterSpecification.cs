// Copyright (c) 2012-2018 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Actors
{
    /// <summary>
    /// RouterSpecification specifies the definition and protocol of
    /// the <see cref="Actor"/> to which a <see cref="Router"/> will route,
    /// as well as other details such as pool size.
    /// </summary>
    public class RouterSpecification
    {
        private readonly int poolSize; //TODO: refactor towards resizable pool
        private readonly Definition routerDefinition;
        private readonly Type routerProtocol;

        public RouterSpecification(int poolSize, Definition routerDefinition, Type routerProtocol)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentException("poolSize must be 1 or greater");
            }
            this.poolSize = poolSize;
            this.routerDefinition = routerDefinition;
            this.routerProtocol = routerProtocol;
        }

        public virtual int PoolSize => poolSize;

        public virtual Definition RouterDefinition => routerDefinition;

        public virtual Type RouterProtocol => routerProtocol;
    }
}
