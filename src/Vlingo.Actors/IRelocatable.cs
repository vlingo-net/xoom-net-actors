// Copyright © 2012-2020 Vaughn Vernon. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Actors
{
    public interface IRelocatable
    {
        /// <summary>
        /// Sets my <typeparamref name="S"/> typed state snapshot, which by default does nothing.
        /// Override to set a snapshot state.
        /// </summary>
        /// <param name="stateSnapshot">The <typeparamref name="S"/> typed state snapshot to set</param>
        /// <typeparam name="S">The type of the state snapshot</typeparam>
        void StateSnapshot<S>(S stateSnapshot);
        
        /// <summary>
        /// Answer my <typeparamref name="S"/> typed state snapshot, which is <code>null</code> by default.
        /// Override to set a snapshot state.
        /// </summary>
        /// <typeparam name="S">The type of the state snapshot</typeparam>
        S StateSnapshot<S>();
    }
}