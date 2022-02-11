// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

namespace Vlingo.Xoom.Actors;

internal class Addressable__Proxy : IAddressable
{
    private readonly Actor actor;

    public Addressable__Proxy(Actor actor, IMailbox mailbox)
    {
        this.actor = actor;
    }

    public IAddress Address => actor.Address;

    public LifeCycle LifeCycle => actor.LifeCycle;
}