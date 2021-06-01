// Copyright © 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors
{
    /// <summary>
    /// Defines the interface for all <see cref="Actor"/> proxies. The Actor's <see cref="IAddress"/> is
    /// available as well as <code>Equals()</code>, <code>GetHashCode()</code>, and <code>ToString()</code>.
    /// </summary>
    public interface IProxy
    {
        /// <summary>
        /// Gets the underlying <see cref="Actor"/> <see cref="IAddress"/>
        /// </summary>
        IAddress Address { get; }
    }
    
    public static class ProxyExtensions
    {
        public static IProxy FromRaw(this object proxy) => (IProxy) proxy;
    }
}